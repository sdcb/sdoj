namespace question {
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

        setCodeMode(mode: string) {
            this.code.session.setMode(mode);
        }

        setCode(text: string) {
            this.code.setValue(text);
        }

        onLanguageChange(onchange: () => void) {
            this.languageSelect.change(() => onchange());
        }

        onCodeChange(onchange: () => void) {
            this.code.session.on("change", () => onchange());
        }

        getQuestionId(): string {
            return (<HTMLInputElement>document.getElementById("questionId")).value;
        }
                
        getSourceCode() {
            return this.code.getValue();
        }

        getXSRFToken() {
            return $("[name=__RequestVerificationToken]").val();
        }

        getXSRFTokenObject() {
            return {
                __RequestVerificationToken: this.getXSRFToken()
            }
        }
    }

    export class QuestionDetailsPage {
        dom = new Dom();
        isDefaultTemplate = ko.observable<boolean>(false);
        edited = ko.observable<boolean>(false);

        constructor() {
            $.post("/solution/getPerferedLanguage").then((lang: string) => {
                const language = Languages[lang];
                this.dom.setSelectedLanguage(language);
                this.updateCode();
            });
            this.dom.onLanguageChange(() => {
                this.updateCode();
            });
            this.dom.onCodeChange(() => {
                this.isDefaultTemplate(false);
                this.edited(true);
            });
        }

        reset() {
            if (confirm("确定还原到默认代码模板？")) {
                const url = `/solution/ResetCodeTemplate?questionId=${this.dom.getQuestionId()}&language=${this.dom.getSelectedLanguage()}`;
                $.post(url, this.dom.getXSRFTokenObject()).then(data => {
                    this.updateCode();
                });
            }
        }

        save() {
            const url = `/solution/SaveCodeTemplate?questionId=${this.dom.getQuestionId()}&language=${this.dom.getSelectedLanguage()}`;
            $.post(url, $.extend(this.dom.getXSRFTokenObject(), { code: this.dom.getSourceCode() })).then(data => {
                this.updateCode();
            });
        }

        updateCode() {
            this.dom.setCodeMode(languageToAceMode(this.dom.getSelectedLanguage()));
            $.post(`/solution/codeTemplate?questionId=${this.dom.getQuestionId()}&language=${this.dom.getSelectedLanguage()}`).then(template => {
                this.dom.setCode(template.code);
                this.isDefaultTemplate(template.isDefault);
                this.edited(false);
            });
        }
    }
}