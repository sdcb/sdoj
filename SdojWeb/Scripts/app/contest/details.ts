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
        code = ko.observable<string>();
        compilerOutput = ko.observable<string>();
        solutionId = ko.observable<number>();

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
            const $solutionDiv = $("#solutions");
            $solutionDiv.load(`/contest/details/${dom.getContestId()}/question-${dom.getQuestionId()}/solutions`, null, () => {
                ko.applyBindingsToNode($solutionDiv[0], this);
            });
        }

        loadCode(solutionId: number) {
            this.code("加载中...");
            this.solutionId(solutionId);
            $.get(`/solution/source/${solutionId}`).then(data => {
                this.code(data.replace(new RegExp("\t", "g"), "    "));
            });
        }

        rawCode() {
            open(`/solution/source/${this.solutionId()}`);
        }

        showCompilerOutput(solutionId: number) {
            $("#compiler-modal").modal();
            this.compilerOutput('加载中...');
            this.solutionId(solutionId);
            $.post(`/solution/compilerOutput/${solutionId}`, function (data) {
                this.compilerOutput(data);
            });
        }

        showWrongAnswer(solutionId: number) {
            $("#compiler-modal").modal();
            this.compilerOutput('加载中...');
            this.solutionId(solutionId);
            $.post(`/solution/wrongAnswer/${solutionId}`).then(data => {
                if (data.Exists) this.compilerOutput("输入：" + data.Input + "\r\n" + "你的输出（错误）：" + data.Output);
                else this.compilerOutput("<数据不存在或已删除。>");
            });
        }
    }
}