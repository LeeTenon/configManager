import pb from 'protobufjs'

class PBTool {
    pbRef: pb.Namespace
    pbOriObj: any
    pbObj: any
    enums: {
        [k: string]: any
    }
    constructor() {
        this.pbRef = {} as pb.Namespace
        this.pbOriObj = {}
        this.pbObj = {}
        this.enums = {}
    }

    resolveProto(src: string[]) {
        this.pbRef = pb.parse(src[0]).root
        for (let i = 1; i < src.length; i++) {
            this.pbRef = this.pbRef.addJSON(pb.parse(src[i]).root.toJSON().nested!)
        }

        let pbObject = this.pbRef.toJSON()
        this.pbOriObj = pbObject.nested![Object.keys(pbObject.nested!)[0]]["nested"]

        this.resolve()
    }

    private resolve() {
        for (let field in this.pbOriObj) {
            if (this.pbOriObj[field]["fields"] == undefined) {
                this.enums[field] = this.pbOriObj[field]["values"]
            } else {
                this.pbObj[field] = this.checkField(this.pbOriObj[field]["fields"])
            }
        }
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
                        "#value#": 0,
                        "#type#": "number",
                    }
                    break;
                case "string":
                    obj[field] = {
                        "#value#": "",
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

    // private checkField(src: any): any {
    //     let obj = {}
    //     for (let field in src) {
    //         //数组
    //         if (src[field]["rule"] == "repeated") {
    //             if (src[field]["type"] == "string") {
    //                 obj[field] = ["#string#"]
    //             } else {
    //                 obj[field] = ["#number#"]
    //             }
    //             continue
    //         }
    //         //非数组
    //         switch (src[field]["type"]) {
    //             case "int":
    //                 obj[field] = 0
    //                 break;
    //             case "string":
    //                 obj[field] = ""
    //                 break;
    //             default:
    //                 let h = this.findType(src[field]["type"])
    //                 if (field == "_") {  //继承字段
    //                     for (let i in h) {
    //                         obj[i] = h[i]
    //                     }
    //                 } else {
    //                     obj[field] = h
    //                 }
    //                 break;
    //         }
    //     }
    //     return obj
    // }

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
                "#value#": 0,
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