enum Languages {
    csharp = 1,
    vb = 2,
    cpp = 3,
    c = 4,
    python3 = 5,
    java = 6,
}

declare var ace: IAceStatic;

interface IAceStatic {
    edit(element: HTMLElement, options: IAceOptions): IAce;
}

interface IAce {
    getValue(): string;
    setValue(code: string);
    session: IAceSession;
}

interface IAceSession {
    setMode(mode: string);
    on(eventName: string, handler: () => void);
}

interface IAceOptions {
    mode: string;
}

class PerferedLanguageStore {
    static key = "perferedLanguage";
    set(language: Languages) {
        localStorage[PerferedLanguageStore.key] = language;
    }

    get(): Languages {
        return parseInt(localStorage[PerferedLanguageStore.key]) || Languages.csharp;
    }
}

function languageToAceMode(language: Languages) {
    switch (language) {
        case Languages.csharp: return 'ace/mode/csharp';
        case Languages.c:
        case Languages.cpp: return 'ace/mode/c_cpp';
        case Languages.vb: return 'ace/mode/vbscript';
        case Languages.python3: return 'ace/mode/python';
        case Languages.java: return 'ace/mode/java';
        default: return 'ace/mode/text';
    }
}

function getLanguageTemplate(language: Languages) {
    let languageName = Languages[language];
    let element = document.getElementById(`language-template-${languageName}`);

    if (!element) {
        console.warn(`No template for ${languageName}.`);
        return "";
    }

    return element.innerText;
}