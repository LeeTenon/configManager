import pb from 'protobufjs'

class PBTool {
    pbRef: pb.Namespace
    pbOriObj: any
    pbObj: any
    pbCommonObj: any
    enums: {
        [k: string]: any
    }
    constructor() {
        this.pbRef = {} as pb.Namespace
        this.pbOriObj = {}
        this.pbObj = {}
        this.pbCommonObj = {}
        this.enums = {}
    }

    resolveProto(src: any) {
        let common = {}

        let first = true
        for (let file in src) {
            if (file == 'common') {
                let tmp = pb.parse(src[file]).root.toJSON()
                common = tmp.nested![Object.keys(tmp.nested!)[0]]['nested']['_Common']['fields']
                continue
            }
            if (first) {
                this.pbRef = pb.parse(src[file]).root
                first = false
            } else {
                this.pbRef = this.pbRef.addJSON(pb.parse(src[file]).root.toJSON().nested!)
            }
        }

        let pbObject = this.pbRef.toJSON()
        this.pbOriObj = pbObject.nested![Object.keys(pbObject.nested!)[0]]["nested"]
        this.resolve()
        this.resolveCommon(common)
    }

    private resolve() {
        for (let field in this.pbOriObj) {
            if ('fields' in this.pbOriObj[field]) {
                this.pbObj[field] = this.checkField(this.pbOriObj[field]["fields"])
            } else {
                this.enums[field] = this.pbOriObj[field]["values"]
            }
        }
    }

    private resolveCommon(common: any) {
        this.pbCommonObj = this.checkField(common)
    }

    private checkField(src: any): any {
        let obj = {}
        for (let field in src) {
            //数组
            if (src[field]["rule"] == "repeated") {
                if (src[field]["type"] == "string") {
                    obj[field] = {
                        "#value#": [],
                        "#type#": "slice",
                        "spec": "string"
                    }
                } else {
                    obj[field] = {
                        "#value#": [],
                        "#type#": "slice",
                        "spec": "number"
                    }
                }
                continue
            }
            //非数组
            switch (src[field]["type"]) {
                case "int64":
                    obj[field] = {
                        "#value#": null,
                        "#type#": "number",
                    }
                    break;
                case "string":
                    obj[field] = {
                        "#value#": null,
                        "#type#": "string",
                    }
                    break;
                default:
                    let h = this.findType(src[field]["type"])
                    if (field == "_") {  //继承字段
                        for (let i in h) {
                            obj[i] = h[i]
                        }
                    } else {
                        obj[field] = h
                    }
                    break;
            }
        }
        return obj
    }

    private findType(tName: string): any {
        if (!(tName in this.pbOriObj)) { //说明不是顶级域定义的结构，为内嵌定义
            console.log("内嵌结构:", tName)
        } else {
            if (tName in this.pbObj) {   //已经解析
                return this.pbObj[tName]
            }
            if ("fields" in this.pbOriObj[tName]) {
                return this.checkField(this.pbOriObj[tName]["fields"])
            }
            return {
                "#value#": null,
                "#type#": "enum",
                "spec": tName
            }
        }
    }
}

export function loadPB(src: string[]): PBTool {
    let tool = new PBTool
    tool.resolveProto(src)
    return tool
}