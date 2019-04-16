enum Languages {
    csharp  = 1,
    vb      = 2,
    cpp     = 3,
    c       = 4,
    python3 = 5,
    java    = 6,
}

class Dom {
    private languageSelect = $("select[name='Language']");
    private code = $("textarea[name='Source']");

    getSelectedLanguage() {
        return <Languages>parseInt(this.languageSelect.val());
    }

    setSelectedLanguage(language: Languages) {
        this.languageSelect.val(language.toString());
    }

    setSourceCode(text: string) {
        this.code.val(text);
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
        dom.setSourceCode(getLanguageTemplate(dom.getSelectedLanguage()));
    });
    dom.setSourceCode(getLanguageTemplate(dom.getSelectedLanguage()));

    function getLanguageTemplate(language: Languages) {
        let languageName = Languages[language];
        let element = document.getElementById(`language-template-${languageName}`);

        if (!element) {
            console.warn(`No template for ${languageName}.`);
            return "";
        }

        return element.innerText;
    }
})();