var MediaPlayer = {
    PlayAudio: function (filename) {
        const mp3 = '/audio/' + filename + '.mp3';
        const wav = '/audio/' + filename + '.wav';

        let audio = new Audio();
        audio.src = audio.canPlayType('audio/mpeg') ? mp3 : wav;
        audio.play();
    }
};
