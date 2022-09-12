# Aisistant
for hackathon

## Building
npm init can be run from the wwwroot/js directory
Then
Browserify must be run if you modify the aiagent file:
browserify aiagent.js --s aiagent -o bundle.js

The VSCode build/run tasks should then be setup to run the netcore application