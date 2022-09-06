package main

import (
    "time"
)

type Challenge struct {
    BaseConfig
    Spec struct {
        ServerCallTimeout   time.Duration `json:",default=100ms"`
        RewardUnlockTimeout time.Duration `json:",default=100ms"`
    }
}

type Assets struct {
    BaseConfig
}

type BaseConfig struct {
    Name     string
    Mode     string
    Database struct {
        Mysql struct {
            Host string
        }
        Redis struct {
            Host    string
            Timeout time.Duration
            Shu     int
        }
    }
}
