package logx

import (
    "bytes"
)

var GloLog Logger

type Logger struct {
    buf bytes.Buffer
}

func init() {
    var buf bytes.Buffer
    GloLog = Logger{
        buf: buf,
    }
}

func (l *Logger) Info(s string) {
    l.buf.WriteString(s + "\n")
}

func (l *Logger) GetLogs() string {
    var log = make([]byte, l.buf.Len())
    _, err := l.buf.Read(log)
    if err != nil {
        return ""
    }
    return string(log)
}
