# Aisistant
for hackathon

## Prerequeisites

- Netcore 6 SDK
- npm
- browserify
- working webcam and microphone
- RevAI API key
- CoHere API key

## Building
from the wwwroot/js directory:
```
cd wwwroot/js
npm install
browserify aiagent.js --s aiagent -o bundle.js
```

The VSCode build/run tasks should then be setup to run the netcore application
