enum Languages {
    csharp  = 1,
    vb      = 2,
    cpp     = 3,
    c       = 4,
    python3 = 5,
    java    = 6,
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
}

interface IAceOptions {
    mode: string;
}

class Dom {
    private languageSelect = $("select[name='Language']");
    private code = ace.edit($(".code")[0], {
        mode: "ace/mode/javascript"
    });

    constructor() {
        $(".code textarea").attr('name', 'Source');
    }

    getSelectedLanguage() {
        return <Languages>parseInt(this.languageSelect.val());
    }

    setSelectedLanguage(language: Languages) {
        this.languageSelect.val(language.toString());
    }

    setSourceCode(mode: string, text: string) {
        this.code.session.setMode(mode);
        this.code.setValue(text);
        $(".code textarea").val(text).change();
    }

    onLanguageChange(onchange: () => void) {
        this.languageSelect.change(() => onchange());
    }
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

(function () {
    let dom = new Dom();
    let store = new PerferedLanguageStore();

    dom.setSelectedLanguage(localStorage.perferedLanguage || Languages.csharp);
    dom.onLanguageChange(() => {
        localStorage.perferedLanguage = store.get();
        dom.setSourceCode(languageToAceMode(dom.getSelectedLanguage()), getLanguageTemplate(dom.getSelectedLanguage()));
    });
    dom.setSourceCode(languageToAceMode(dom.getSelectedLanguage()), getLanguageTemplate(dom.getSelectedLanguage()));
})();

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