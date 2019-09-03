import MemeTvApi from "/assets/js/api.js";

const video = document.querySelector('.video-player');

let player = undefined;
let lastSubtitleCount = 0;

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

    player = new Plyr('.video-player', {
        captions: {
            active: true,
            language: 'en'
        },
        controls: ['play-large', 'play', 'progress', 'current-time', 'mute', 'volume', 'fullscreen'],
        autoplay: true
    });
};

const loadParodyVideoEditor = async () => {
    if (typeof player === 'undefined') {
        player = new Plyr('.video-player', {
            captions: {
                active: true,
                language: 'en'
            },
            controls: ['play-large', 'play', 'progress', 'current-time', 'mute', 'volume', 'fullscreen'],
            autoplay: false
        });
    }
    const inputName = document.getElementById('inputName');
    const inputEmail = document.getElementById('inputEmail');
    const generatedLink = document.getElementById('generatedLink');

    const btnCreate = document.querySelector('.btn-create');
    const subtitleInputs = [];
    const subtitleList = document.querySelector('.subtitle-list');
    for (let i = 0; i < 10; ++i) {
        const input = document.createElement('input');
        input.id = `subtitle-${i}`;
        input.type = 'text';
        input.placeholder = `Caption ${i+1}`;
        input.style.display = 'none';
        input.addEventListener("change", () => {
            const v = document.querySelector("video");
            v.textTracks[0].cues[i].text = input.value;
        });
        subtitleInputs.push(input);
        subtitleList.appendChild(input);
    }

    const result = await MemeTvApi.getClipsAsync();
    const selectableClips = await result.json();

    const isValidEmail = (email) => {
        const regex = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return regex.test(email);
    };

    const showValidationError = (nameError, emailError) => {
        if (nameError) {
            inputName.classList.add('validation-error');
        }

        if (emailError) {
            inputEmail.classList.add('validation-error');
        }
    };

    const showSaveResult = (result) => {
        const createResultDiv = document.querySelector('.create-result');
        createResultDiv.style.display = 'block';
        generatedLink.href = window.location.href + result.id;
        generatedLink.innerHTML = window.location.href + result.id;
    };

    const showUnknownSaveError = () => {
        alert('unable to save your parody right now. :( Please try again later');
    };

    const selectClip = (clip) => {
        player.source = {
            type: 'video',
            sources: [{
                src: '/data/clips/mp4/clip' + clip.name + '.mp4',
                type: 'video/mp4'
            }, {
                src: '/data/clips/webm/clip' + clip.name + ".webm",
                type: 'video/webm'
            }],
            poster: '/data/clips/images/grandeclip' + clip.name + '.jpg',
            tracks: [{
                kind: 'captions',
                label: 'Parody',
                srclang: 'en',
                src: '/api/subtitles/vtt/empty/' + clip.name,
                default: true
            }]
        };

        const v = document.querySelector("video");
        subtitleInputs.forEach((x, i) => {
            x.style.display = i < clip.subtitleCount ? 'block' : 'none';
            x.value = '';
        });

        lastSubtitleCount = clip.subtitleCount;

        btnCreate.onclick = async () => {
            const inputs = subtitleInputs.slice(0, lastSubtitleCount);
            const captions = [...inputs.map(x => x.value)];
            const name = inputName.value;
            const email = inputEmail.value;

            inputName.classList.remove('validation-error');
            inputEmail.classList.remove('validation-error');

            const isEmailValid = isValidEmail(email);
            if (name.length == 0 || !isEmailValid) {
                showValidationError(name.length == 0, !isEmailValid);
                return;
            }

            const response = await MemeTvApi.saveSubtitlesAsync(name, email, `clip${clip.name}`, captions);
            if (response.ok) {
                const result = await response.json();
                showSaveResult(result);
            } else {
                showUnknownSaveError();
            }
        };
    };
    selectClip(selectableClips[0]);

    const videoSelector = document.querySelector(".video-selector");
    videoSelector.innerHTML = '';
    selectableClips.forEach(x => {
        const img = document.createElement("img");
        const name = `clip${x.name}`;
        img.src = x.image;
        img.setAttribute("data-clip", name);
        img.addEventListener("click", () => {
            selectClip(x);
        });
        videoSelector.appendChild(img);
    });
};

if (typeof id !== 'undefined') {
    loadUserParodyVideo(id);
} else {
    loadParodyVideoEditor();
}