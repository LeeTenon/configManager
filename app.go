package main

import (
    "context"
    "encoding/json"
    "io"
    "io/ioutil"
    "myproject/common/logx"
    configManager "myproject/configManage"
    "os"
)

type App struct {
    ctx       context.Context
    configMgr *configManager.ConfigManager
}

type appConfig struct {
    configManager.ConfigManager
}

func NewApp() *App {
    return &App{}
}

func (a *App) startup(ctx context.Context) {
    logx.InitLogger(ctx)

    conf := &appConfig{}
    data, err := ioutil.ReadFile(AppConfigPath)
    if err != nil {
        if os.IsNotExist(err) { // 生成配置文件模板
            tmp, _ := json.MarshalIndent(conf, "", "\t")
            f, _ := os.Create(AppConfigPath)
            _, _ = io.WriteString(f, string(tmp))
        }

    } else {
        err = json.Unmarshal(data, conf)
        if err != nil {

        }
    }

    a.configMgr = configManager.NewConfigManager(&conf.ConfigManager)
}

func (a *App) LoadProto() *Response {
    data, err := a.configMgr.LoadProto()
    if err != nil {
        return &Response{Error: err.Error()}
    }
    return &Response{Data: data}
}

func (a *App) LoadConfigCache() *Response {
    data, err := a.configMgr.LoadCache()
    if err != nil {
        return &Response{Error: err.Error()}
    }
    return &Response{Data: data}
}

func (a *App) SaveConfig(in string) *Response {
    if err := a.configMgr.Save(in); err != nil {
        return &Response{Error: err.Error()}
    }
    return &Response{}
}

func (a *App) GenConfig(in map[string]string) *Response {
    if err := a.configMgr.Generate(in); err != nil {
        return &Response{Error: err.Error()}
    }
    return &Response{}
}

func (a *App) SyncData() *Response {
    if err := a.configMgr.SyncData(); err != nil {
        return &Response{Error: err.Error()}
    }
    return &Response{}
}

type Response struct {
    Error string
    Data  interface{}
}
