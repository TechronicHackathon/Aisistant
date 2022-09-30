# Aisistant
for hackathon

## Prerequisites

- Netcore 6 SDK
- npm
- browserify
- working webcam and microphone
- RevAI API key
- CoHere API key

## Setup

- Download the Dotnet 6 **SDK** : https://dotnet.microsoft.com/en-us/download/dotnet/6.0
- Download VSCode or Visual Studio: https://code.visualstudio.com
- VSCode will prompt you to install dotnet 6 extensions to help with formatting/code completion, etc, accept and install these

### Env file

The application uses a secret .env file with API keys. You should have a unique API key from CoHere playground and RevAI labs
Once you have these keys, create a file called ".env" in the root of your project, and add it as below:

```
TESTAPIKEY=<cohereAPI>
TESTAPIKEY2=<revLabsKey>
```
## Building
from the wwwroot/js directory:
```
cd wwwroot/js
npm install
browserify aiagent.js --s aiagent -o bundle.js
```

The VSCode build/run tasks should then be setup to run the netcore application

## Contributing

The "Controllers" directory is where the API controllers live. Currently it is just using standard MVC controllers, but this could potentially be augmented with Swagger for .NET

The "Services" directory is the API controllers for talking to the CoHere API and Wikipedia

The "Pages" folder has the HTML with Razor syntax for creating front-end UI
