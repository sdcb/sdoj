namespace Contest {
    class Dom {
        private code = ace.edit($(".code")[0], {
            mode: "ace/mode/csharp"
        });
        private languageSelect = $("#language");

        getSelectedLanguage() {
            return <Languages>parseInt(this.languageSelect.val());
        }

        setSelectedLanguage(language: Languages) {
            this.languageSelect.val(language.toString());
        }

        setSourceCode(mode: string, text: string) {
            this.code.session.setMode(mode);
            this.code.setValue(text);
        }

        onLanguageChange(onchange: () => void) {
            this.languageSelect.change(() => onchange());
        }

        getQuestionId(): number {
            return $(".nav li.active").data("id");
        }

        getContestId(): number {
            return $(".navbar-brand").data("id");
        }

        getSourceCode() {
            return this.code.getValue();
        }
    }

    const dom = new Dom();
    const store = new PerferedLanguageStore();
    dom.setSelectedLanguage(localStorage.perferedLanguage || Languages.csharp);
    dom.onLanguageChange(() => {
        localStorage.perferedLanguage = store.get();
        dom.setSourceCode(languageToAceMode(dom.getSelectedLanguage()), getLanguageTemplate(dom.getSelectedLanguage()));
    });
    dom.setSourceCode(languageToAceMode(dom.getSelectedLanguage()), getLanguageTemplate(dom.getSelectedLanguage()));

    export class DetailsModel {
        constructor() {
            this.loadSolutions();
        }

        submit() {
            let language = dom.getSelectedLanguage();
            let code = dom.getSourceCode();
            if (code.length > 32 * 1024) return;
            $.post(`/contest/details/${dom.getContestId()}/question-${dom.getQuestionId()}/submit`, {
                language: language, 
                source: code, 
            }).then(data => {
                this.loadSolutions();
            });
        }

        loadSolutions() {
            $("#solutions").load(`/contest/details/${dom.getContestId()}/question-${dom.getQuestionId()}/solutions`);
        }
    }
}