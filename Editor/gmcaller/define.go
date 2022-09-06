package gmcaller

type Caller interface {
    Login(string) interface{}
    CallMethod(CallInfo) interface{}
}

type CallInfo struct {
    Name    string `json:"name"`
    Content string `json:"content"`
    UserId  string `json:"userId"`
}
