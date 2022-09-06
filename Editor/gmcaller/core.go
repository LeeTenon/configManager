package gmcaller

import (
    "encoding/json"
    "fmt"
    "io/ioutil"
    "net/http"
    "net/url"
    "strings"
)

var GlobalCaller *GMCaller

type GMCaller struct {
    ID       string `json:"id"`
    Password string `json:"password"`
    Env      string `json:"env"`
    Token    string `json:"token"`
}
type loginResp struct {
    Error string `json:"error"`
    Token string `json:"token"`
}
type callResp struct {
    Error  string `json:"error"`
    Result string `json:"result"`
}

func GenerateGmCaller(id string, password string, env string) error {
    GlobalCaller = &GMCaller{
        ID:       id,
        Password: password,
        Env:      env,
    }
    if err := GlobalCaller.Login(); err != nil {
        return err
    }

    return nil
}

func (c *GMCaller) Login() error {
    url := fmt.Sprintf("%s/api/login?id=%s&password=%s", c.Env, c.ID, c.Password)
    resp, err := http.Get(url)
    if err != nil {
        return err
    }
    defer resp.Body.Close()

    respBytes, err := ioutil.ReadAll(resp.Body)
    if err != nil {
        return err
    }

    var response = new(loginResp)
    _ = json.Unmarshal(respBytes, response)
    GlobalCaller.Token = response.Token

    return nil
}

func (c *GMCaller) Menu() interface{} {
    url := fmt.Sprintf("%s/api/method", c.Env)
    request, _ := http.NewRequest("GET", url, nil)
    request.Header.Add("Authorization", c.Token)

    client := &http.Client{}
    resp, err := client.Do(request)
    if err != nil {
        return err
    }
    defer resp.Body.Close()

    respBytes, err := ioutil.ReadAll(resp.Body)
    if err != nil {
        return err
    }

    return string(respBytes)
}

func (c *GMCaller) CallMethod(name, content, userId string) interface{} {
    baseUrl := c.Env + "/api/method/call"
    Furl := url.Values{}
    Furl.Set("name", name)
    Furl.Set("content", content)
    Furl.Set("userId", userId)

    buffer := strings.NewReader(Furl.Encode())
    reqest, err := http.NewRequest("POST", baseUrl, buffer)
    if err != nil {
        return err
    }
    reqest.Header.Add("Authorization", c.Token)
    reqest.Header.Set("Content-Type", "application/x-www-form-urlencoded")

    client := &http.Client{}
    resp, _ := client.Do(reqest)
    defer resp.Body.Close()
    if resp.StatusCode == 401 {
        if err := c.Login(); err != nil {
            return err
        }
        return c.CallMethod(name, content, userId)
    }
    respBytes, err := ioutil.ReadAll(resp.Body)
    if err != nil {
        return err
    }
    respBody := &callResp{}
    _ = json.Unmarshal(respBytes, respBody)

    return respBody
}

func (c *GMCaller) Reset() interface{} {
    baseUrl := c.Env + "/api/resetroot"
    Furl := url.Values{}
    Furl.Set("resetSecret", "reset999xxx")

    buffer := strings.NewReader(Furl.Encode())
    reqest, err := http.NewRequest("POST", baseUrl, buffer)
    if err != nil {
        return err
    }
    reqest.Header.Set("Content-Type", "application/x-www-form-urlencoded")

    client := &http.Client{}
    resp, _ := client.Do(reqest)
    defer resp.Body.Close()

    if resp.StatusCode != 200 {
        return resp.StatusCode
    }
    respBytes, err := ioutil.ReadAll(resp.Body)
    if err != nil {
        return err
    }

    return string(respBytes)
}
