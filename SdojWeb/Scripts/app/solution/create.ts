namespace solution {
    class Dom {
        private languageSelect = $("select[name='Language']");
        private code = ace.edit($(".code")[0], {
            mode: "ace/mode/javascript"
        });
        private hiddenCode = $("input[name='Source']");
        private form = <HTMLFormElement>document.getElementById("codeForm");

        constructor() {
            this.form.addEventListener("submit", () => {
                this.hiddenCode.val(this.code.getValue());
            });
        }

        getSelectedLanguage() {
            return <Languages>parseInt(this.languageSelect.val());
        }

        setSelectedLanguage(language: Languages) {
            this.languageSelect.val(language.toString());
        }

        setCodeMode(mode: string) {
            this.code.session.setMode(mode);
        }

        setCode(text: string) {
            this.code.setValue(text);
        }

        onLanguageChange(onchange: () => void) {
            this.languageSelect.change(() => onchange());
        }

        getQuestionId() {
            return (<HTMLInputElement>document.getElementById("questionId")).value;
        }
    }

    let dom = new Dom();

    dom.onLanguageChange(() => {
        dom.setCodeMode(languageToAceMode(dom.getSelectedLanguage()));
        $.post(`/solution/codeTemplate?questionId=${dom.getQuestionId()}&language=${dom.getSelectedLanguage()}`).then(template => {
            dom.setCode(template.code);
        });
    });
    dom.setCodeMode(languageToAceMode(dom.getSelectedLanguage()));
    $.post(`/solution/codeTemplate?questionId=${dom.getQuestionId()}&language=${dom.getSelectedLanguage()}`).then(template => {
        dom.setCode(template.code);
    });
}