import MemeTvApi from "/assets/js/api.js";

const video = document.querySelector('.video-player');

let player = undefined;
let lastSubtitleCount = 0;

const loadUserParodyVideo = async (id) => {
    video.innerHTML = '';
    video.setAttribute("autoplay", "");
    const creatorName = document.querySelector('.creator-name');
    const viewsCount = document.querySelector(".view-count");
    const likeCount = document.querySelector(".like-count");
    const shareModal = document.querySelector('.share-modal');
    const btnLike = document.querySelector('.like');
    const btnShare = document.querySelector('.share');
    const parodyTitle = document.querySelector('.parody-title');

    const subtitles = await MemeTvApi.getSubtitlesAsync(id);
    const data = await subtitles.json();
    const sourceMp4 = document.createElement("source");
    const sourceWebM = document.createElement("source");

    sourceMp4.src = data.mp4;
    sourceWebM.src = data.webm;
    sourceMp4.type = 'video/mp4';
    sourceWebM.type = 'video/webm';

    viewsCount.innerHTML = data.views + '';
    likeCount.innerHTML = data.likes + '';
    if (data.title !== null && data.title.length > 0)
        parodyTitle.innerText = data.title + ' - ';
    document.querySelector('.parody-description').innerHTML = data.description;
    document.querySelector('.share-facebook').onclick = () => {
        window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodeURI(window.location.href)}`);
    };
    document.querySelector('.share-twitter').onclick = () => {
        window.open(`https://twitter.com/intent/tweet?url=${encodeURI(window.location.href)}&text=${encodeURI('A parody by ' + data.name)}&via=MemeTV&related=MemeTV`);
    };
    document.querySelector('.share-reddit').onclick = () => {
        window.open(`http://www.reddit.com/submit?url=${encodeURI(window.location.href)}&title=${encodeURI('A parody by ' + data.name)}`);
    };
    document.querySelector('.share-modal .content').onclick = ev => {
        ev.stopPropagation();
        ev.preventDefault();
        event.cancelBubble = true;
    };

    shareModal.onclick = ev => {
        shareModal.classList.remove('visible');
    };

    btnShare.onclick = ev => {
        shareModal.classList.add('visible');
        document.querySelector(".share-url").innerHTML = window.location.href.toString();
    };

    btnLike.onclick = async () => {
        const result = await MemeTvApi.likeAsync(id);
        if (!result.ok) return;
        const likeResult = await result.json();
        btnLike.classList.remove('liked');
        if (likeResult.liked) {
            btnLike.classList.add('liked');
        }
        likeCount.innerHTML = likeResult.likes + '';
    };

    if (data.liked) {
        btnLike.classList.add('liked');
    } else {
        btnLike.classList.remove('liked');
    }

    window.document.head.title = 'Meme-TV - Parody by ' + data.name;
    creatorName.innerHTML = data.name;

    player = new Plyr('.video-player', {
        captions: { active: true, language: 'en' },
        controls: ['play-large', 'play', 'progress', 'current-time', 'mute', 'volume', 'fullscreen'],
        autoplay: true
    });

    player.source = {
        type: 'video',
        sources: [{ src: data.mp4, type: 'video/mp4' }, { src: data.webm, type: 'video/webm' }],
        poster: data.image,
        tracks: [{ kind: 'captions', label: 'Parody', srclang: 'en', src: data.vtt, default: true }]
    };
};

const loadParodyVideoEditor = async () => {
    if (typeof player === 'undefined') {
        player = new Plyr('.video-player', {
            captions: { active: true, language: 'en' },
            controls: ['play-large', 'play', 'progress', 'current-time', 'mute', 'volume', 'fullscreen'],
            autoplay: false
        });
    }
    const inputName = document.getElementById('inputName');
    const inputEmail = document.getElementById('inputEmail');
    const generatedLink = document.getElementById('generatedLink');
    const btnCreate = document.querySelector('.btn-create');
    const report = document.querySelector('.btn-bad-subtitles');
    const reportResult = document.querySelector('.report-bad-subtitle-result');
    const subtitleInputs = [];
    const subtitleList = document.querySelector('.subtitle-list');
    for (let i = 0; i < 10; ++i) {
        const input = document.createElement('input');
        input.id = `subtitle-${i}`;
        input.type = 'text';
        input.placeholder = `Caption ${i + 1}`;
        input.style.display = 'none';
        input.addEventListener("focus", () => {
            const cue = document.querySelector("video").textTracks[0].cues[i];
            player.currentTime = cue.startTime;
        });
        input.addEventListener("keydown", () => document.querySelector("video").textTracks[0].cues[i].text = input.value);
        input.addEventListener("change", () => document.querySelector("video").textTracks[0].cues[i].text = input.value);
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
        report.style.display = 'block';
        reportResult.innerHTML = '';
        player.source = {
            type: 'video',
            sources: [
                { src: '/data/clips/mp4/clip' + clip.name + '.mp4', type: 'video/mp4' },
                { src: '/data/clips/webm/clip' + clip.name + ".webm", type: 'video/webm' }
            ],
            poster: '/data/clips/images/grandeclip' + clip.name + '.jpg',
            tracks: [
                { kind: 'captions', label: 'Parody', srclang: 'en', src: '/api/subtitles/vtt/empty/' + clip.name, default: true }
            ]
        };

        subtitleInputs.forEach((x, i) => {
            x.style.display = i < clip.subtitleCount ? 'block' : 'none';
            x.value = '';
        });

        lastSubtitleCount = clip.subtitleCount;

        report.onclick = async () => {
            await MemeTvApi.reportBadCaptionsAsync(clip.name);
            report.style.display = 'none';
            reportResult.innerHTML = 'Thank you for reporting the issue!';
        };

        btnCreate.onclick = async () => {
            const inputs = subtitleInputs.slice(0, lastSubtitleCount);
            const captions = [...inputs.map(x => x.value)];
            const name = inputName.value;
            const email = inputEmail.value;

            const title = document.getElementById("inputTitle").value;
            const description = document.getElementById("inputDescription").value;

            inputName.classList.remove('validation-error');
            inputEmail.classList.remove('validation-error');

            const isEmailValid = isValidEmail(email);
            if (name.length === 0 || !isEmailValid) {
                showValidationError(name.length === 0, !isEmailValid);
                return;
            }

            const response = await MemeTvApi.saveSubtitlesAsync(name, email, title, description, `clip${clip.name}`, captions);
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
        img.addEventListener("click", () => selectClip(x));
        videoSelector.appendChild(img);
    });
};

if (typeof id !== 'undefined') {
    loadUserParodyVideo(id);
} else {
    loadParodyVideoEditor();
}
