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
                this.pbObj[field] = this.checkField(this.pbOriObj[field]["fields"], field)
            } else {
                this.enums[field] = this.pbOriObj[field]["values"]
            }
        }
    }

    private resolveCommon(common: any) {
        this.pbCommonObj = this.checkField(common, '')
    }

    private checkField(src: any, topName: string): any {
        let obj = {}
        for (let field in src) {
            //数组
            if (src[field]["rule"] == "repeated") {
                if (src[field]["type"] == "string") {
                    obj[field] = {
                        "#value#": undefined,
                        "#type#": "slice",
                        "spec": "string"
                    }
                } else {
                    obj[field] = {
                        "#value#": undefined,
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
                        "#value#": undefined,
                        "#type#": "number",
                    }
                    break;
                case "string":
                    obj[field] = {
                        "#value#": undefined,
                        "#type#": "string",
                    }
                    break;
                case 'bool':
                    obj[field] = {
                        "#value#": undefined,
                        "#type#": "bool",
                    }
                    break
                default:
                    let h = this.findType(src[field]["type"], topName)
                    if (field[0] == "_" && field[1] == '_') {  //继承字段
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

    private findType(tName: string, topName: string): any {

        if (topName != '' && 'nested' in this.pbOriObj[topName] && tName in this.pbOriObj[topName]['nested']) {  //message内部定义
            return this.checkField(this.pbOriObj[topName]['nested'][tName]['fields'], topName)
        }
        if (tName in this.pbOriObj) {   //顶级域定义
            if (tName in this.pbObj) {
                return this.pbObj[tName]
            }
            if ("fields" in this.pbOriObj[tName]) {
                return this.checkField(this.pbOriObj[tName]["fields"], topName)
            }
            return {
                "#value#": undefined,
                "#type#": "enum",
                "spec": tName
            }
        }
        console.log('类型错误')
        return {
            "#value#": '类型错误, 请检查proto文件',
            "#type#": "",
            "spec": tName
        }
    }
}

export function loadPB(src: string[]): PBTool {
    let tool = new PBTool
    tool.resolveProto(src)
    return tool
}