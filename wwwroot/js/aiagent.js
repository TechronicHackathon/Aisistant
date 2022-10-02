const revai = require('revai-node-sdk');
const token = revaikey;
const fabricjs = fabric;
let $;
// initialize client with audio configuration and access token
const audioConfig = new revai.AudioConfig(
    /* contentType */ "audio/x-raw",
    /* layout */      "interleaved",
    /* sample rate */ 48000,
    /* format */      "F32LE",
    /* channels */    1
);

// initialize microphone configuration
// note: microphone device id differs
// from system to system and can be obtained with
// arecord --list-devices and arecord --list-pcms
const micConfig = {
    /* sample rate */ rate: 16000,
    /* channels */    channels: 1,
    /* device id */   device: 'hw:0,0'
};

let streamingClient;
let captions = new fabricjs.Text("");
captions.top = 1000;
captions.left = 33;
captions.fill = "white";
captions.backgroundColor = "black";
captions.fontSize = 35;

async function startAIAgent(jquery) {
  $ = jquery;
  console.log("starting AI");
  var client = new revai.RevAiStreamingClient(token, audioConfig);


  // create microphone stream

  //micStream.setStream(canstream)
  // create event responses
  client.on('close', (code, reason) => {
    console.log(`Connection closed, ${code}: ${reason}`);
  });
  client.on('httpResponse', code => {
    console.log(`Streaming client received http response with code: ${code}`);
  });
  client.on('connectFailed', error => {
    console.log(`Connection failed with error: ${error}`);
  });
  client.on('connect', connectionMessage => {
    console.log(`Connected with message: ${connectionMessage}`);
  });
  // micStream.on('error', error => {
  //   console.log(`Microphone input stream error: ${error}`);
  // });

  // micStream.on('data', function (chunk) {
  //   console.log("mic data reced")
  //   console.log(chunk)

  // });

  // create event responses

  // pipe the microphone audio to Rev AI client
  //URL: wss://api.rev.ai/speechtotext/v1/stream?access_token=02B0zyHcAMcvEb_bjY1LiRJ2mkjNksoSordP3Xvi-WSdzL2tKYIpTs2j0OaITf2R-lZv5uA9MOPl8O-49vtzkkSGWtcI4&
  //content_type=audio/x-raw;layout=interleaved;rate=16000;format=S16LE;channels=1&user_agent=RevAi-NodeSDK%2F3.5.1

  //URL: wss://api.rev.ai/speechtotext/v1/stream?
  //content_type=audio/x-raw;layout=interleaved;rate=48000;format=F32LE;channels=1&metadata=Submitted%20via%20Web&access_token=oat_CfDJ8GUe23tLxi5Hp_8MqSn9JxBqH8y-B_VpWRKnjX5emALH7frwvsxdcdUEEUNKCtz4Hjkj6f70ltMG-UDd3c8tUM9aGxup-V9zusnyOG52jyq0cdEpGspcJHnQuF54eNbFy6c_09mvQlzbK56gMzDn3aDZKQLQYLir--FaIAbsUA96fdKYyVFPMHPnSRRhamr2L2RRVsPDBefVXTET1ckhWZQiC8WL-iBubJGHp0VvndmZP0Pi4ONt-mLnhCQJV6j6ycWZNODNQVacITdU952DbK-uAEG-hOfIFXgOd8mFf_HG&filter_profanity=true&language=en&transcriber=machine_v2

  var revstream = client.start();
  streamingClient = revstream.client;
  var micMediaSource = await navigator.mediaDevices.getUserMedia({
    audio: !0
  });
  streamingClient.onmessage = (msg) => {
    //console.log(JSON.parse(msg.data));
    processTranscription(JSON.parse(msg.data));
  };
  revstream.protocol.on('data', data => {
    console.log(data);
  });
  revstream.protocol.on('end', function () {
    console.log("End of Stream");
  });

  setupAudioContext(micMediaSource);
  fcanvas.add(captions);

  // Forcibly ends the streaming session
  // stream.end();
}
function showUserMessage(htmlMessage) {
  let newToast = $(htmlMessage);
  $(".toast-container").append(newToast);
  newToast.toast("show");
}

function getInterestingMsg() {
  $.get("/apiview/GetInterestingMessage").done(
    (result) => {
      if (result.type != null) console.log(result);
      else showUserMessage(result);
    });
}
function logToServer(lastSentence, startT_S, endT_S) {

  $.ajax({
    url: "/api/Log",
    type: "POST",
    dataType: "json", // expected format for response
    contentType: "application/json", // send as JSON
    data: JSON.stringify({ text: lastSentence, startT_S: startT_S, endT_S: endT_S }),

    complete: function () {
      //called when complete
    },

    success: function (dat) {
      //called when successful
      console.log(dat);
      var delaytime = (Math.random() * 5) + 5;
      setTimeout(getInterestingMsg, 1000 * delaytime);
    },

    error: function (dat) {
      //called when there is an error
      console.log(dat);

    },
  });

}
function processTranscription(transData) {
  //"{\"type\":\"final\",\"ts\":6.67,\"end_ts\":9.31,
  //\"elements\":[{\"type\":\"text\",\"value\":\"Test\",\"ts\":6.67,\"end_ts\":7.16,\"confidence\":0.97},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"one\",\"ts\":7.16,\"end_ts\":7.4,\"confidence\":0.87},{\"type\":\"punct\",\"value\":\",\"},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"two\",\"ts\":7.4,\"end_ts\":7.68,\"confidence\":0.97},{\"type\":\"punct\",\"value\":\",\"},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"test\",\"ts\":7.68,\"end_ts\":8.0,\"confidence\":0.97},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"one\",\"ts\":8.0,\"end_ts\":8.32,\"confidence\":0.78},{\"type\":\"punct\",\"value\":\",\"},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"two\",\"ts\":8.32,\"end_ts\":8.56,\"confidence\":0.97},{\"type\":\"punct\",\"value\":\".\"}]}" = $5
  if (transData.type == "final") {
    let lastSentence = "";
    for (let i = 0; i < transData.elements.length; i++) {
      lastSentence += transData.elements[i].value;
    }
    console.log(lastSentence);
    captions.text = lastSentence;
    logToServer(lastSentence, transData.ts, transData.end_ts);
    //displaycaption

  } else if (transData.type == "partial") {
    let lastWord = "";
    for (let i = 0; i < transData.elements.length; i++) {
      lastWord += transData.elements[i].value + " ";
    }
    console.log(lastWord);
    captions.text = lastWord;

  }

}
var audioContext;
function setupAudioContext(audioStreamID) {
  audioContext = new (window.AudioContext || window.webkitAudioContext);
  audioContext.suspend();
  var micStream = audioStreamID;
  var scriptNode = audioContext.createScriptProcessor(4096, 1, 1);
  var input = audioContext.createMediaStreamSource(micStream)
  scriptNode.addEventListener("audioprocess", processAudioEvent)
  input.connect(scriptNode)
  scriptNode.connect(audioContext.destination)
  audioContext.resume();

}

function processAudioEvent(e) {
  if ("suspended" !== audioContext.state && "closed" !== audioContext.state && streamingClient) {
    var t = e.inputBuffer.getChannelData(0),
      n = containsAudio(t);
    //isnonenglish
    false ? streamingClient.send(convertToPCM(t)) : streamingClient.send(t)
  }
}
function convertToPCM(e) {
  for (var t = new DataView(new ArrayBuffer(2 * e.length)), n = 0; n < e.length; n++) {
    var r = e[n] < 0 ? 32768 : 32767;
    t.setInt16(2 * n, e[n] * r | 0, !0)
  }
  for (var a = new Int16Array(t.buffer), i = a.length; i-- && 0 === a[i] && i > 0;)
    ;
  return a.slice(0, i + 1)
}
function containsAudio(e) {
  for (var t = 0; t < e.length; t++)
    if (0 !== e[t])
      return !0;
  return !1
}
exports.startAIAgent = startAIAgent