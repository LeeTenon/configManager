package configManager

import (
    "io"
    "io/ioutil"
    "myproject/common/logx"
    "os"
    "path"
    "strings"
)

type (
    ConfigManager struct {
        OutputPath    string
        CachePath     string
        ProtoPath     string
        SyncSourceDir string
        SyncTargetDir string
        SyncList      []string
    }
)

func NewConfigManager(c *ConfigManager) *ConfigManager {
    return c
}

func (c *ConfigManager) LoadProto() (map[string]string, error) {
    files, err := ioutil.ReadDir(c.ProtoPath)
    if err != nil {
        logx.Errorf("load proto file error: %s", err.Error())
        return nil, err
    }

    protos := make(map[string]string)
    for _, file := range files {
        if !file.IsDir() {
            logx.Infof("读取proto配置文件: %s", file.Name())
            data, err := ioutil.ReadFile(path.Join(c.ProtoPath, file.Name()))
            if err != nil {
                logx.Errorf("load proto file[%s] error: %s", file.Name(), err.Error())
                continue
            }
            protos[file.Name()[:strings.LastIndexByte(file.Name(), '.')]] = string(data)
        }
    }
    return protos, nil

}

func (c *ConfigManager) LoadCache() (string, error) {
    data, err := os.ReadFile(c.CachePath)
    if err != nil {
        logx.Errorf("open file error: %s", err.Error())
        return "", err
    }
    logx.Infof("load template success")

    return string(data), nil
}

func (c *ConfigManager) Save(dataJson string) error {
    err := writeFile("", c.CachePath, []byte(dataJson))
    if err != nil {
        logx.Errorf("save config error: %s", err.Error())
        return err
    }
    logx.Infof("save config complete!")

    return nil
}

func (c *ConfigManager) Generate(yamlData map[string]string) error {
    for serviceName, data := range yamlData {
        err := writeFile(path.Join(c.OutputPath, serviceName), serviceName+".yaml", []byte(data))
        if err != nil {
            logx.Errorf("generate yaml[%s] error: %s", serviceName, err.Error())
        } else {
            logx.Infof("generate %s config success", serviceName)
        }
    }
    logx.Infof("generate config complete!")

    return nil
}

func (c *ConfigManager) SyncData() error {
    // 同步数据表
    for _, name := range c.SyncList {
        content, err := ioutil.ReadFile(path.Join(c.SyncSourceDir, name))
        if err != nil {
            logx.Errorf("read csv[%s] error: %s", name, err.Error())
            continue
        }
        if err = writeFile(c.SyncTargetDir, name, content); err != nil {
            logx.Errorf("write csv[%s] error: %s", name, err.Error())
            continue
        }
        logx.Infof("sync csv data [%s] success", name)
    }
    logx.Infof("sync data files complete, total: %d files!", len(c.SyncList))

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
