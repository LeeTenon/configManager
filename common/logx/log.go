package logx

import (
    "context"
    "fmt"
    "github.com/wailsapp/wails/v2/pkg/runtime"
)

type Logger struct {
    ctx context.Context
}

var defaultLogger = &Logger{}

func InitLogger(ctx context.Context) {
    defaultLogger.ctx = ctx
}

func Errorf(format string, arg ...interface{}) {
    format = "ERROR: " + format
    runtime.EventsEmit(defaultLogger.ctx, "log-event", fmt.Sprintf(format, arg...))
}

func Infof(format string, arg ...interface{}) {
    runtime.EventsEmit(defaultLogger.ctx, "log-event", fmt.Sprintf(format, arg...))
}
