package configManager

import (
    "context"
    "encoding/json"
    "fmt"
    "io"
    "io/ioutil"
    "myproject/pkg/logx"
    "os"
    "path"
    "reflect"
    "sync"

    "github.com/gitHusband/goutils/jsonkeys"
    "gopkg.in/yaml.v2"
)

const (
    // configTemplatePath = "NBServer/tools/configManager/template.json"
    // configCachePath    = "NBServer/tools/configManager/cache.json"
    configTemplatePath = "../template.json"
    configCachePath    = "../cache.json"
    csvSource          = "Programs/ClientData/CSVData/"
    jsonSource         = "Programs/ClientData/JsonData/"
    csvDir             = "NBServer/data/CSVData/"
    jsonDir            = "NBServer/data/JsonData/"
    syncConfig         = "NBServer/tools/configManager/sync.yaml"

    configDir = "NBServer/config/"
)

type ConfigManager struct {
    sync.Mutex
    template map[string]interface{}
}

var Manager = &ConfigManager{
    template: make(map[string]interface{}),
}

func (c *ConfigManager) Save(ctx context.Context, dataJson string) error {
    err := writeFile("", configCachePath, []byte(dataJson))
    if err != nil {
        logx.GloLog.Info(fmt.Sprintf("Save config error: %s", err.Error()))
        return err
    }
    logx.GloLog.Info("Save config complete!")

    return nil
}

func (c *ConfigManager) LoadTemplate(ctx context.Context) (string, error) {
    data, err := os.ReadFile(configTemplatePath)
    if err != nil {
        logx.GloLog.Info(fmt.Sprintf("open file error: %s", err.Error()))
        return "", err
    }
    return string(data), nil
}

func (c *ConfigManager) LoadCache(ctx context.Context) (string, error) {
    data, err := os.ReadFile(configCachePath)
    if err != nil {
        logx.GloLog.Info(fmt.Sprintf("open file error: %s", err.Error()))
        return "", err
    }
    return string(data), nil
}

func (c *ConfigManager) Generate(mode string) error {
    for serviceName, config := range c.template {
        target := getTarget(config.(map[string]interface{})[mode])
        result, err := yaml.Marshal(target)
        if err != nil {
            logx.GloLog.Info(fmt.Sprintf("Marshal yaml[%s] error: %s", serviceName, err.Error()))
            continue
        }
        err = writeFile(path.Join(configDir, serviceName), serviceName+".yaml", result)
        if err != nil {
            logx.GloLog.Info(fmt.Sprintf("Generate yaml[%s] error: %s", serviceName, err.Error()))
        }
    }
    logx.GloLog.Info("Generate config complete!")

    return nil
}

type table struct {
    Csv  []string `yaml:"csv"`
    Json []string `yaml:"json"`
}

func (c *ConfigManager) SyncCsv() error {
    // 读取需要读取的表
    data, err := ioutil.ReadFile(syncConfig)
    if err != nil {
        logx.GloLog.Info(fmt.Sprintf("Load csvConfig error: %s", err))
        return err
    }
    tableInfo := &table{
        Csv:  make([]string, 0),
        Json: make([]string, 0),
    }
    if err = yaml.Unmarshal(data, tableInfo); err != nil {
        logx.GloLog.Info(fmt.Sprintf("Unmarshall csvConfig error: %s", err.Error()))
        return err
    }
    // 同步Csv
    for _, name := range tableInfo.Csv {
        content, err := ioutil.ReadFile(path.Join(csvSource, name))
        if err != nil {
            logx.GloLog.Info(fmt.Sprintf("Read file[%s] error: %s", name, err.Error()))
            continue
        }
        if err = writeFile(csvDir, name, content); err != nil {
            logx.GloLog.Info(fmt.Sprintf("Write file error: %s", err))
        }
    }
    // 同步Json
    for _, name := range tableInfo.Json {
        content, err := ioutil.ReadFile(path.Join(jsonSource, name))
        if err != nil {
            logx.GloLog.Info(fmt.Sprintf("Read file[%s] error: %s", name, err.Error()))
            continue
        }
        if err = writeFile(jsonDir, name, content); err != nil {
            logx.GloLog.Info(fmt.Sprintf("Write file error: %s", err.Error()))
        }
    }

    logx.GloLog.Info("Sync csv success!")
    return nil
}

func getTarget(data interface{}) interface{} {
    target := make(map[string]interface{})
    for k, v := range data.(map[string]interface{}) {
        if reflect.TypeOf(v).Kind() == reflect.Map {
            sub := getTarget(v).(map[string]interface{})
            if len(sub) != 0 {
                target[k] = sub
            }
            continue
        }
        if reflect.TypeOf(v).Kind() == reflect.Slice && len(v.([]interface{})) == 0 {
            continue
        }
        if v == nil || v == "" || v == float64(0) || v == false {
            continue
        }
        target[k] = v
    }
    return target
}

func (c *ConfigManager) loadTemplate() error {
    data, err := os.ReadFile(configTemplatePath)
    if err != nil {
        logx.GloLog.Info(fmt.Sprintf("open file error: %s", err.Error()))
        return err
    }
    jsMap, _ := jsonkeys.ParseFromData(data)
    logx.GloLog.Info(fmt.Sprintf("Marshal config error: %#v", jsMap))
    err = json.Unmarshal(data, &c.template)
    if err != nil {
        logx.GloLog.Info(fmt.Sprintf("open file error: %s", err.Error()))
        return err
    }

    return nil
}

func setValue(dst, src map[string]interface{}) {
    for key, value := range src {
        if _, ok := dst[key]; ok {
            if reflect.TypeOf(value).Kind() == reflect.Map && reflect.TypeOf(dst[key]).Kind() == reflect.Map {
                setValue(dst[key].(map[string]interface{}), value.(map[string]interface{}))
            } else {
                dst[key] = value
            }
        }
    }
}

func writeFile(p, filename string, content []byte) error {
    filePath := path.Join(p, filename)
    logx.GloLog.Info(fmt.Sprintf("write: %s", filePath))
    var f *os.File
    if _, err := os.Stat(filePath); os.IsNotExist(err) {
        f, err = os.Create(filePath) //创建文件
        if err != nil {
            return err
        }
    } else if err != nil {
        return err
    } else {
        _ = os.Chmod(filePath, 0666)
        f, err = os.OpenFile(filePath, os.O_RDWR|os.O_TRUNC, 0666) //打开文件
        if err != nil {
            return err
        }
    }
    _, err := io.WriteString(f, string(content)) //写入文件(字符串)
    if err != nil {
        return err
    }
    _ = f.Close()

    return nil
}
