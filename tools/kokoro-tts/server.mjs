import http from 'node:http';
import path from 'node:path';
import fs from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { KokoroTTS } from 'kokoro-js';
import { env } from '@huggingface/transformers';

const directory = path.dirname(fileURLToPath(import.meta.url));
env.cacheDir = path.join(directory, '.cache', 'models');
const voice = 'af_heart';
const speed = 0.85;
const port = Number(process.env.KOKORO_PORT || 17787);
const cache = new Map();
let ready = false;
let loadFailed = false;
let queue = Promise.resolve();
let pending = 0;

function encodeWav(audio) {
  const samples = audio.audio;
  const wav = Buffer.alloc(44 + samples.length * 2);
  wav.write('RIFF', 0);
  wav.writeUInt32LE(wav.length - 8, 4);
  wav.write('WAVEfmt ', 8);
  wav.writeUInt32LE(16, 16);
  wav.writeUInt16LE(1, 20);
  wav.writeUInt16LE(1, 22);
  wav.writeUInt32LE(audio.sampling_rate, 24);
  wav.writeUInt32LE(audio.sampling_rate * 2, 28);
  wav.writeUInt16LE(2, 32);
  wav.writeUInt16LE(16, 34);
  wav.write('data', 36);
  wav.writeUInt32LE(samples.length * 2, 40);
  for (let i = 0; i < samples.length; i++) {
    const sample = Math.max(-1, Math.min(1, samples[i]));
    wav.writeInt16LE(Math.round(sample * (sample < 0 ? 32768 : 32767)), 44 + i * 2);
  }
  return wav;
}

const model = KokoroTTS.from_pretrained('onnx-community/Kokoro-82M-v1.0-ONNX', {
  dtype: 'q8', device: 'cpu',
}).then(async tts => {
  // Prime the phonemizer and voice before reporting readiness.
  const sample = encodeWav(await tts.generate('Hello.', { voice, speed }));
  cache.set('Hello.', sample);
  ready = true;
  console.log(`Kokoro ready: ${voice}, speed ${speed}`);
  return tts;
});
model.catch(error => {
  loadFailed = true;
  console.error('Kokoro model could not be loaded:', error);
});

if (process.argv.includes('--warmup')) {
  await model;
  const samplePath = path.join(directory, '.cache', 'sample.wav');
  await fs.mkdir(path.dirname(samplePath), { recursive: true });
  const tts = await model;
  await fs.writeFile(samplePath, encodeWav(await tts.generate(
    'Apple. Algorithm. English pronunciation.', { voice, speed })));
  console.log(`Sample saved: ${samplePath}`);
} else {
  const sendJson = (res, status, data) => {
    res.writeHead(status, { 'Content-Type': 'application/json', 'Cache-Control': 'no-store' });
    res.end(JSON.stringify(data));
  };

  const server = http.createServer(async (req, res) => {
    if (req.method === 'GET' && req.url === '/health') {
      sendJson(res, 200, { engine: 'kokoro', ready, failed: loadFailed, voice, speed });
      return;
    }
    if (req.method !== 'POST' || req.url !== '/speech') {
      sendJson(res, 404, { error: 'Not found' });
      return;
    }
    try {
      const chunks = [];
      let size = 0;
      for await (const chunk of req) {
        size += chunk.length;
        if (size > 4096) {
          sendJson(res, 413, { error: 'Request too large' });
          return;
        }
        chunks.push(chunk);
      }
      let text;
      try { text = JSON.parse(Buffer.concat(chunks).toString('utf8')).text; }
      catch { sendJson(res, 400, { error: 'Invalid JSON' }); return; }
      if (typeof text !== 'string' || !text.trim() || text.trim().length > 200) {
        sendJson(res, 400, { error: 'Text must contain 1 to 200 characters' });
        return;
      }
      text = text.trim();
      let wav = cache.get(text);
      if (!wav) {
        if (pending >= 8) {
          sendJson(res, 429, { error: 'Please try again shortly' });
          return;
        }
        pending++;
        // Serialize inference; eSpeak's phonemizer shares state within the process.
        const work = queue.then(async () => {
          if (res.destroyed) return null;
          const tts = await model;
          return encodeWav(await tts.generate(text, { voice, speed }));
        });
        queue = work.catch(() => {});
        try { wav = await work; }
        finally { pending--; }
        if (!wav || res.destroyed) return;
        if (cache.size >= 128) cache.delete(cache.keys().next().value);
        cache.set(text, wav);
      }
      res.writeHead(200, { 'Content-Type': 'audio/wav', 'Content-Length': wav.length,
        'Cache-Control': 'no-store', 'X-Speech-Engine': 'kokoro', 'X-Speech-Voice': voice });
      res.end(wav);
    } catch (error) {
      console.error('Speech generation failed:', error);
      if (!res.headersSent && !res.destroyed)
        sendJson(res, 503, { error: 'Kokoro speech is unavailable' });
    }
  });
  server.requestTimeout = 180_000;
  server.listen(port, '127.0.0.1', () => console.log(`Kokoro listening on 127.0.0.1:${port}`));
  server.on('error', error => { console.error(error); process.exit(1); });

  const parentIndex = process.argv.indexOf('--parent-pid');
  const parentPid = Number(process.argv[parentIndex + 1]);
  if (parentIndex >= 0 && Number.isInteger(parentPid) && parentPid > 0) {
    setInterval(() => {
      try { process.kill(parentPid, 0); }
      catch { process.exit(0); }
    }, 5000).unref();
  }
}
