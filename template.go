package main

import (
    "time"
)

type ConfigTemplate struct {
    BaseConfig
    Spec interface{} `json:",omitempty"`
    Mode string
}

func getChallengeSpec() interface{} {
    return &struct {
        ServerCallTimeout string
    }{}
}

type BaseConfig struct {
    Name     string
    Etcd     []string
    Database struct {
        Mysql struct {
            Host []string
        }
        Redis struct {
            Host    []string
            Timeout time.Duration
            Shu     int
        }
    }
}
