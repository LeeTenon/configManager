import pb from 'protobufjs'

export function loadPB(src: Object): { [serviceName: string]: Object } {
    let configs = {}

    let pbData = new pb.Namespace('empty')
    for (let file in src) {
        pbData.add(pb.parse(src[file]).root.nestedArray[0])
    }
    let structMap = pbData.resolveAll().nestedArray[0]['nested']
    for (let name in structMap)
        if (name.includes('_Config'))
            configs[name.substring(0, name.indexOf('_'))] = toObject(structMap[name])

    console.log(configs)
    return configs
}

function toObject(pbType: pb.Type): Object {
    let c: { [k: string]: Object } = {}
    for (let field of pbType.fieldsArray) {
        if (field.resolvedType != null) {   // 关联类型
            if (field.resolvedType instanceof pb.Type) {   // 结构体
                let sub = toObject(field.resolvedType)
                if (field.name == '__') {
                    for (let subField in sub) {
                        c[subField] = sub[subField]
                    }
                } else {
                    c[field.name] = sub
                }
            } else if (field.resolvedType instanceof pb.Enum) {    // 枚举
                c[field.name] = new tail('enum', field.resolvedType.values)
            }
        } else {
            if (field.repeated) {
                c[field.name] = new tail(field.type, 'array')
            } else {
                c[field.name] = new tail(field.type, '')
            }

        }
    }
    return c
}

class tail {
    $type: string
    $spec: string | Object

    constructor(type: string, spec: string | Object) {
        this.$type = type
        this.$spec = spec
    }
}

interface enumOption {
    lable: string,
    value: number | string
}

function genEnumArray(src: { [k: string]: number }): enumOption[] {
    let options = new Array<enumOption>
    for (let eStr in src) {
        options.push({
            lable: eStr,
            value: eStr,
        })
    }
    return options
}