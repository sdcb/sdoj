enum Languages {
    Csharp = 1,
    Vb = 2,
    Cpp = 3,
    C = 4,
    Python3 = 5,
    Java = 6,
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

function languageToAceMode(language: Languages) {
    switch (language) {
        case Languages.Csharp: return 'ace/mode/csharp';
        case Languages.C:
        case Languages.Cpp: return 'ace/mode/c_cpp';
        case Languages.Vb: return 'ace/mode/vbscript';
        case Languages.Python3: return 'ace/mode/python';
        case Languages.Java: return 'ace/mode/java';
        default: return 'ace/mode/text';
    }
}