export default class MemeTvApi {
    static async getSubtitlesAsync(id) {
        return await MemeTvApi.sendAsync(`subtitles/${id}`);
    }

    static async saveSubtitlesAsync(name, email, title, description, clip, subtitles) {
        return await MemeTvApi.sendAsync('subtitles/', { name, email, title, description, clip, subtitles });
    }

    static async getClipsAsync() {
        return await MemeTvApi.sendAsync('subtitles/clips');
    }

    static async reportBadCaptionsAsync(clip) {
        return await MemeTvApi.sendAsync('subtitles/report/' + clip);
    }

    static async likeAsync(clip) {
        return await MemeTvApi.sendAsync('subtitles/like/' + clip);
    }

    static async sendAsync(method, data = null) {
        const url = MemeTvApi.buildRequestUrl(method);
        if (data !== null) {
            return await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });
        } 
        return await fetch(url);        
    }

    static buildRequestUrl(method) {
        return `/api/${method}`;
    }
}