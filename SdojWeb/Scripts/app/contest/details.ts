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

        setCodeMode(mode: string) {
            this.code.session.setMode(mode);
        }

        setCode(text: string) {
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

        getRestTimeInSeconds(): number {
            return $(document.body).data("restTime");
        }

        submitTimeout() {
            $("#timeoutForm").submit();
        }
    }

    const dom = new Dom();
    dom.onLanguageChange(() => {
        dom.setCodeMode(languageToAceMode(dom.getSelectedLanguage()));
        $.post(`/solution/codeTemplate?questionId=${dom.getQuestionId()}&language=${dom.getSelectedLanguage()}`).then(template => {
            dom.setCode(template);
        });
    });
    dom.setCodeMode(languageToAceMode(dom.getSelectedLanguage()));
    $.post(`/solution/codeTemplate?questionId=${dom.getQuestionId()}&language=${dom.getSelectedLanguage()}`).then(template => {
        dom.setCode(template);
    });

    export class DetailsModel {
        code = ko.observable<string>();
        compilerOutput = ko.observable<string>();
        solutionId = ko.observable<number>();
        restTime = ko.observable<number>();
        dom = dom;

        restTimeText = ko.pureComputed(() => {
            let rt = this.restTime();
            let hours = Math.floor(rt / (60 * 60));
            let minutes = Math.floor(rt / 60);
            let seconds = Math.floor(rt % 60);
            return `${hours}:${pad2(minutes)}:${pad2(seconds)}`;

            function pad2(v: number) {
                if (v < 10) return `0${v}`;
                return v;
            }
        });

        constructor() {
            this.loadSolutions();
            this.initSignalR();
            this.initRestTime();
        }

        initRestTime() {
            this.restTime(dom.getRestTimeInSeconds());
            const intervalId = setInterval(() => {
                this.restTime(this.restTime() - 1);
                if (this.restTime() <= 0) {
                    clearInterval(intervalId);
                    dom.submitTimeout();
                }
            }, 1000);
        }

        initSignalR() {
            var shub = $.connection.solutionHub;
            shub.client.push = (id, name, runtime, memory) => {
                $("#runtime-" + id).text(runtime);
                $("#memory-" + id).text(memory.toFixed(2));
                if (name === "答案错误") {
                    $("#state-" + id).html(`<a href="javascript:void(0);" data-bind="click: showWrongAnswer.bind($data, ${id})">${name}</a>`);
                    ko.applyBindingsToDescendants(this, $("#state-" + id)[0]);
                } else if (name === "编译失败") {
                    $("#state-" + id).html(`<a href="javascript:void(0);" data-bind="click: showCompilerOutput.bind($data, ${id})">${name}</a>`);
                    ko.applyBindingsToDescendants(this, $("#state-" + id)[0]);
                } else {
                    $("#state-" + id).text(name);
                }
            };
            $.connection.hub.start();
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
                ko.applyBindingsToDescendants(this, $solutionDiv[0]);
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
            $.post(`/solution/compilerOutput/${solutionId}`).then((data) => {
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

interface JQueryStatic {
    connection: any;
}