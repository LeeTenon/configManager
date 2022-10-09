package main

import (
    "encoding/json"
    "io"
    "log"
    "os"
    "reflect"
)

var (
    mode     = []string{"dev", "test", "qa", "pro"}
    services = map[string]interface{}{ //key -> 对应yaml文件的名称
        "challenge": initStruct(&ConfigTemplate{Spec: getChallengeSpec()}),
        "assets":    initStruct(&ConfigTemplate{Spec: getChallengeSpec()}),
    }
)

func main() {
    configs := make(map[string]map[string]interface{})
    for service, configTemplate := range services {
        configs[service] = make(map[string]interface{})
        for _, m := range mode {
            configs[service][m] = configTemplate
        }
    }
    data, _ := json.MarshalIndent(configs, "", "\t")

    filePath := "./template.json"
    var f *os.File
    if _, err := os.Stat(filePath); os.IsNotExist(err) {
        f, _ = os.Create(filePath) //创建文件
    } else {
        _ = os.Chmod(filePath, 0666)
        f, err = os.OpenFile(filePath, os.O_RDWR|os.O_TRUNC, 0666) //打开文件
        if err != nil {
            log.Println("open file fail: ", err.Error())
        }
    }

    _, err := io.WriteString(f, string(data)) //写入文件(字符串)
    if err != nil {
        log.Println("open file fail: ", err.Error())
    }

    _ = f.Close()
}

func initStruct(data *ConfigTemplate) interface{} {
    t := reflect.TypeOf(data).Elem()
    v := reflect.ValueOf(data).Elem()
    subStruct(t, v)
    return data
}
func subStruct(t reflect.Type, v reflect.Value) {
    for i := 0; i < t.NumField(); i++ {
        if t.Field(i).Type.Kind() == reflect.Slice {
            v.Field(i).Set(reflect.ValueOf(make([]string, 0)))
        }
        if t.Field(i).Type.Kind() == reflect.Struct {
            subStruct(t.Field(i).Type, v.Field(i))
        }
    }
}

func setDefault(c *ConfigTemplate, serviceName string, mode string) {
    c.Name = "rpc." + serviceName
    c.Mode = mode
}
