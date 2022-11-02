import pb from 'protobufjs'

class PBTool {

}

export function loadPB(src: Object): { [k: string]: Object } {
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
                c[field.name] = toObject(field.resolvedType)
            } else if (field.resolvedType instanceof pb.Enum) {    // 枚举
                c[field.name] = 'enum'
                // console.log(field.resolvedType)
            }
        } else {
            c[field.name] = field.type
        }
    }
    return c
}