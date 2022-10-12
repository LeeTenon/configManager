package configManager

import (
    "io"
    "io/ioutil"
    "myproject/common/logx"
    "os"
    "path"
    "sync"

    "gopkg.in/yaml.v2"
)

type ConfigManager struct {
    sync.Mutex
    template map[string]interface{}
}

func NewConfigManager() *ConfigManager {
    return &ConfigManager{
        template: make(map[string]interface{}),
    }
}

func (c *ConfigManager) LoadTemplate() (string, error) {
    data, err := os.ReadFile(configTemplatePath)
    if err != nil {
        logx.Errorf("open file error: %s", err.Error())
        return "", err
    }
    logx.Infof("load template success")

    return string(data), nil
}

func (c *ConfigManager) LoadCache() (string, error) {
    data, err := os.ReadFile(configCachePath)
    if err != nil {
        logx.Errorf("open file error: %s", err.Error())
        return "", err
    }
    logx.Infof("load cache success")

    return string(data), nil
}

func (c *ConfigManager) Save(dataJson string) error {
    err := writeFile("", configCachePath, []byte(dataJson))
    if err != nil {
        logx.Errorf("save config error: %s", err.Error())
        return err
    }
    logx.Infof("save config complete!")

    return nil
}

func (c *ConfigManager) Generate(yamlData map[string]string) error {
    for serviceName, data := range yamlData {
        err := writeFile(path.Join(configDir, serviceName), serviceName+".yaml", []byte(data))
        if err != nil {
            logx.Errorf("generate yaml[%s] error: %s", serviceName, err.Error())
        } else {
            logx.Infof("generate %s config success", serviceName)
        }
    }
    logx.Infof("generate config complete!")

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
        logx.Errorf("load csvConfig error: %s", err)
        return err
    }
    tableInfo := &table{
        Csv:  make([]string, 0),
        Json: make([]string, 0),
    }
    if err = yaml.Unmarshal(data, tableInfo); err != nil {
        logx.Errorf("unmarshall csvConfig error: %s", err.Error())
        return err
    }
    // 同步Csv
    for _, name := range tableInfo.Csv {
        content, err := ioutil.ReadFile(path.Join(csvSource, name))
        if err != nil {
            logx.Errorf("read csv[%s] error: %s", name, err.Error())
            continue
        }
        if err = writeFile(csvDir, name, content); err != nil {
            logx.Errorf("write csv[%s] error: %s", name, err.Error())
            continue
        }
        logx.Infof("sync csv data [%s] success", name)
    }
    // 同步Json
    for _, name := range tableInfo.Json {
        content, err := ioutil.ReadFile(path.Join(jsonSource, name))
        if err != nil {
            logx.Errorf("read json[%s] error: %s", name, err.Error())
            continue
        }
        if err = writeFile(jsonDir, name, content); err != nil {
            logx.Errorf("write json[%s] error: %s", name, err.Error())
            continue
        }
        logx.Infof("sync json data [%s] success", name)
    }
    logx.Infof("sync data files complete, total: %d files!", len(tableInfo.Csv)+len(tableInfo.Json))

    return nil
}

func writeFile(p, filename string, content []byte) error {
    filePath := path.Join(p, filename)
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
