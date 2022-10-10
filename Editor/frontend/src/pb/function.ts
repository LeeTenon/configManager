import pb, { Namespace } from 'protobufjs'

export function toPB(src: string[]): any {
    let oriPB = pb.parse(src[0]).root.addJSON(pb.parse(src[1]).root.toJSON().nested!)
    console.log(oriPB.lookupTypeOrEnum("RpcServerConfig").constructor.name)

    let pbObject = oriPB.toJSON()
    let tmp = pbObject.nested![Object.keys(pbObject.nested!)[0]] as any
    console.log(tmp.nested) 
    
    return  tmp.nested
}