class MediaPlayer
{
    static PlayAudio(filename)
    {
        let mp3 = '/audio/' + filename + '.mp3';
        let wav = '/audio/' + filename + '.wav';

        let audio = new Audio();
        audio.src = audio.canPlayType('audio/mpeg') ? mp3 : wav;
        audio.play();
    }
}
