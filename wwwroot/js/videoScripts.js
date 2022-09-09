console.log("loading video scripts")


var scanvas = document.getElementById("scanvas");

var scontext = scanvas.getContext("2d");
var svideo = document.getElementById("svideohidden");
var screenWidth;
var screenHeight;
var bodypix;
var isPaused = false;
var globalRecHandler;
document.onkeypress = function (evt) {
    evt = evt || window.event;
    var charCode = evt.keyCode || evt.which;
    var charStr = String.fromCharCode(charCode);
    if (charStr == 'p' || charStr == 'P') {
        isPaused = !isPaused;
        var ps = document.getElementById("pauseStat");
        if (isPaused) {
            ps.innerText = "RECORDING PAUSED";
            globalRecHandler.pause();
        } else {
            ps.innerText = "Not Paused";
            globalRecHandler.resume();
        }
    }
};
async function startScreenCapture() {
    await tf.setBackend("webgl");

    if (navigator.getDisplayMedia) {
        return navigator.getDisplayMedia({ video: true });
    } else if (navigator.mediaDevices.getDisplayMedia) {
        return navigator.mediaDevices.getDisplayMedia(
            {
                video: {
                    cursor: "always"
                },
                audio: false
            }
        );
        //          return navigator.mediaDevices.getDisplayMedia({video: true});

    } else {
        
        return navigator.mediaDevices.getUserMedia({ video: { mediaSource: 'screen' } });
    }
}
var fcanvas = new fabric.Canvas('scanvas', {
    hoverCursor: 'pointer',
    selection: false,
    targetFindTolerance: 2
});
fcanvas.setDimensions({ height: 1080 * (.8*window.innerWidth / 1920), width: .8*window.innerWidth }, { cssOnly: true });

async function tick() {
    compatibility.requestAnimationFrame(tick);

    if (svideo.readyState === svideo.HAVE_ENOUGH_DATA) {
        //scontext.drawImage(svideo, 450, 0, 1080, 1920, 0, 0, 720, 1280);
        //  scontext.drawImage(svideo, 0,0,(screenWidth/screenHeight)*1080,1080);
    }
    if (webcam.readyState === webcam.HAVE_ENOUGH_DATA) {
        //scontext.drawImage(svideo, 450, 0, 1080, 1920, 0, 0, 720, 1280);
        // scontext.drawImage(webcam, 0,0);
    }
    //fcanvas.renderAll();
    webcamSprite.render();

}


const webcam = document.getElementById('webcam');
webcam.volume = 0;
const recordBtn = document.getElementById('record');
const loadScreenBtn = document.getElementById('loadScreen');

const canvasCam = document.getElementById('canvasCam');
var webcamSprite = new fabric.Image(canvasCam, {
    left: 1000,
    top: 200,
    scaleX: .7,
    scaleY: .7,
    angle: 0,
    flipX: true,
    originX: 'center',
    originY: 'center',
    objectCaching: false,
});
var screenSprite = new fabric.Image(svideo, {
    left: 0,
    top: 0,
    width: 1920,
    height: 1080,
    angle: 0,
    originX: 'center',
    originY: 'center',
    objectCaching: false,
});

// compatibility.requestAnimationFrame(tick);



/**
 * One of (see documentation below):
 *   - net.segmentPerson
 *   - net.segmentPersonParts
 *   - net.segmentMultiPerson
 *   - net.segmentMultiPersonParts
 * See documentation below for details on each method.
 */

fabric.util.requestAnimFrame(async function render() {

    try {
        //mask = await bodyPix.toMask(segmentation, { r: 255, g: 255, b: 255, a: 255 }, { r: 255, g: 255, b: 255, a: 0 }, false);
        canvasCam.getContext("2d").clearRect(0, 0, canvasCam.width, canvasCam.height);
       // //canvasCam.getContext("2d").globalAlpha = 0.2;
        canvasCam.getContext("2d").globalCompositeOperation = "source-over";
       // bodyPix.drawMask(canvasCam, canvasCam, mask, 1, .2, 0);
        canvasCam.getContext("2d").globalCompositeOperation = "source-in";
        canvasCam.getContext("2d").drawImage(webcam, 0, 0);

    } catch {

    }
    fcanvas.renderAll();

    fabric.util.requestAnimFrame(render);
});
var recMimeType = "video/webm; codecs=h264";
var fileType="webm";
if (MediaRecorder.isTypeSupported != undefined && !MediaRecorder.isTypeSupported(recMimeType)) {
    recMimeType = "video/webm;";
    if(!MediaRecorder.isTypeSupported(recMimeType)){
        recMimeType = "video/mp4;";
        fileType="mp4";
    }
}
var canstream;
async function loadScreenCapture(){
    document.getElementById("output-video").style.display = "none";
    canstream = scanvas.captureStream(); // frames per second
    canstream.addTrack(webcam.srcObject.getAudioTracks()[0]);


    try {
        var sstream = await startScreenCapture();
    } catch (exc) {
        console.log(exc);
        debugger
    }
    svideo.srcObject = sstream;
    var _screenVid = await svideo.play();
    screenHeight = svideo.srcObject.getVideoTracks()[0].getSettings().height;
    screenWidth = svideo.srcObject.getVideoTracks()[0].getSettings().width;
    //screenSprite.scale(1080/screenHeight);
    screenSprite.selectable = false;
    screenSprite.height = 1080;
    screenSprite.width = 1920;
    fcanvas.add(screenSprite);
    screenSprite.sendToBack();
    await aiagent.startAIAgent();


}
const startRecording = async () => {
    var options = {
        mimeType: recMimeType,
        //mimeType: 'video/webm',
        audioBitsPerSecond: 128000,
        videoBitsPerSecond: 3500000,
        audioBitrateMode: 'variable'
    }
    //const rec = new MediaRecorder(webcam.srcObject,options);
    const rec = new MediaRecorder(canstream, options);
    globalRecHandler = rec;
    const chunks = [];


    recordBtn.textContent = 'Stop Recording';
    recordBtn.onclick = () => {
        rec.stop();
        recordBtn.textContent = 'Start Recording';
        recordBtn.onclick = startRecording;
        document.getElementById("output-video").style.display = "unset";
    }

    rec.ondataavailable = e => {
        chunks.push(e.data);
    }
    rec.onstop = async () => {
        var reader = new FileReader();
        const blob = new Blob(chunks);
        reader.onloadend = (event) => {
            transcode(new Uint8Array(reader.result), blob);

        };
        reader.readAsArrayBuffer(blob);


    };
    rec.start();
};

(async () => {
    webcam.srcObject = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
    await webcam.play();
    fcanvas.add(webcamSprite);
    recordBtn.disabled = false;
    loadScreenBtn.onclick=loadScreenCapture;
    recordBtn.onclick = startRecording;

})();

const transcode = async (webcamData, blobs) => {
     downloadBlob(URL.createObjectURL(blobs),fileType);
     return;
    if (MediaRecorder.isTypeSupported == undefined || MediaRecorder.isTypeSupported("video/webm; codecs=h264")) {
        const message = document.getElementById('message');
        const name = 'record.webm';
        message.innerHTML = 'Loading ffmpeg-core.js';
        message.innerHTML = 'Start transcoding';
        await worker.write(name, webcamData);
        var ffmOpts = "-c:a copy -c:v copy";
        //ffmOpts="";
        if (MediaRecorder.isTypeSupported == undefined) {

        } else {
            ffmOpts = "-c:v copy";
        }
        message.innerHTML = 'Complete transcoding';

        const video = document.getElementById('output-video');
        video.src = URL.createObjectURL(new Blob([data.buffer], { type: 'video/mp4' }));
        downloadBlob(video.src, "mp4");

    } else {
        downloadBlob(URL.createObjectURL(blobs), "webm");
    }


}
function downloadBlob(urlSrc, fileExtension) {
    //const url = window.URL.createObjectURL(blobs);
    const a = document.createElement('a');
    a.style.display = 'none';
    a.href = urlSrc;
    a.download = 'recording.' + fileExtension;
    document.body.appendChild(a);
    a.click();
    setTimeout(() => {
        document.body.removeChild(a);
        //window.URL.revokeObjectURL(url);
    }, 100);
}