import MemeTvApi from "/assets/js/api.js";

const video = document.querySelector('.video-player');

let player = undefined;

const loadUserParodyVideo = async (id) => {
    //document.write(JSON.stringify(data));
    video.innerHTML = '';
    video.setAttribute("autoplay", "");

    const subtitles = await MemeTvApi.getSubtitlesAsync(id);
    const data = await subtitles.json();
    const sourceMp4 = document.createElement("source");
    const sourceWebM = document.createElement("source");
    sourceMp4.src = data.mp4;
    sourceWebM.src = data.webm;
    sourceMp4.type = 'video/mp4';
    sourceWebM.type = 'video/webm';

    const vtt = document.createElement("track");
    vtt.label = "English";
    vtt.kind = "subtitles";
    vtt.srclang = "en";
    vtt.src = data.vtt;
    vtt.default = "default";

    video.appendChild(sourceWebM);
    video.appendChild(sourceMp4);
    video.appendChild(vtt);

    player = new Plyr('.video-player',
        {
            captions: { active: true, language: 'en' },
            controls: ['play-large', 'play', 'progress', 'current-time', 'mute', 'volume', 'fullscreen'],
            autoplay: true
        });
};

const loadParodyVideoEditor = async () => {
    if (typeof player === 'undefined') {
        player = new Plyr('.video-player',
            {
                captions: { active: true, language: 'en' },
                controls: ['play-large', 'play', 'progress', 'current-time', 'mute', 'volume', 'fullscreen'],
                autoplay: false
            });
    }

    const selectClip = (clip) => {
        player.source = {
            type: 'video',
            sources: [{
                src: '/data/clips/mp4/' + clip + '.mp4',
                type: 'video/mp4'
            }, {
                src: '/data/clips/webm/' + clip + ".webm",
                type: 'video/webm'
            }],
            poster: '/data/clips/images/grande' + clip + '.jpg',
            tracks: [
                {
                    kind: 'captions',
                    label: 'Parody',
                    srclang: 'en',
                    src: '/api/subtitles/vtt/empty',
                    default: true
                }
            ]
        }
    }; selectClip('clip1');

    const result = await MemeTvApi.getClipsAsync();
    const selectableClips = await result.json();
    const videoSelector = document.querySelector(".video-selector");
    videoSelector.innerHTML = '';
    selectableClips.forEach(x => {
        const img = document.createElement("img");
        const name = `clip${x.name}`;
        img.src = x.image;
        img.setAttribute("data-clip", name);
        img.addEventListener("click", () => {
            selectClip(name);
        });
        videoSelector.appendChild(img);
    });
};

if (typeof id !== 'undefined') {
    loadUserParodyVideo(id);
} else {
    loadParodyVideoEditor();
}
