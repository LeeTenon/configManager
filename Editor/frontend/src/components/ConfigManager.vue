<template>
  <div class="common-layout">
    <el-container>
      <el-header class="header" style="--wails-draggable: drag">
        <div>
          <el-select v-model="mode" class="select" placeholder="请选择导出模式" style="width: 150px; margin-right: 10px">
            <el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value" />
          </el-select>
          <el-button type="primary" @click="GenerateConfig()">生成配置</el-button>
          <el-button type="primary" @click="SaveConfig()">保存配置</el-button>
          <el-button type="primary" @click="SyncDataTable()">同步数据表</el-button>
        </div>
        <Toolbar />
      </el-header>
    </el-container>
    <el-container>
      <el-aside width="150px" class="aside">
        <Logo />
        <el-menu class="menu">
          <el-sub-menu v-for="(configs, service) in treeData" :index="service">
            <template #title>
              <h4>{{ service }}</h4>
            </template>
            <el-menu-item v-for="(config, mode) in configs" :index="service + '-' + mode" @click="handleSelect(service + '-' + mode,service as string,mode as string)">
              <div style="display: flex;width:calc(100% - 30px);">
                <div>
                  {{ mode }}
                </div>
                <div v-if="showDelete == service + '-' + mode" class="delete-buttom" @click="deleteHandle">
                  <el-icon style="margin-left: 5px;">
                    <Close />
                  </el-icon>
                </div>
              </div>
            </el-menu-item>
            <el-popover placement="right" :width="430" trigger="click" :visible="isVisible(service as string)"
              @before-leave="hideHandle">
              <template #reference>
                <div class="add-line" @click="addHandle(service as string)">
                  <el-icon color="white">
                    <Plus />
                  </el-icon>
                </div>
              </template>
              <div class="add-confirm-box">
                复制模板：
                <el-select placeholder="Select" clearable v-model="addTag">
                  <el-option v-for="(config, mode) in configs" :key="mode" :label="mode" :value="mode" />
                </el-select>
                <div style="  margin-left: auto;">
                  <el-button type="primary" @click="addConfig(service as string)">确定</el-button>
                  <el-button type="primary" @click="cancelAdd">取消</el-button>
                </div>
              </div>
            </el-popover>
          </el-sub-menu>
        </el-menu>
      </el-aside>
      <!--数据主体-->
      <el-main>
        <el-table :data="showing" style="width: 100%" :max-height="height" row-key="id" border size="small">
          <el-table-column prop="name" label="配置项" style="width: 50%">
            <template #default="scope">
              <span v-if="scope.row.type=='slice'" style="align-items: center">
                <span>{{ scope.row.name}}</span>
                <el-tag type="info" style="margin-left: 10px" size="small">Slice</el-tag>
              </span>
              <span v-else>{{ scope.row.name }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="value" label="值" style="width: 50%">
            <template #default="scope">
              <div v-if="scope.row.children.length == 0" style="display: flex; align-items: center">
                <el-select v-if="scope.row.type.substring(0,5) == 'enum:'" class="m-2" placeholder="Select" size="small"
                  clearable v-model="scope.row.value">
                  <el-option v-for="(value,key) in enums[scope.row.type.substring(5)]" :key="value" :label="key"
                    :value="key" />
                </el-select>
                <el-input-number v-else-if="typeof scope.row.value == 'number'" v-model="scope.row.value"
                  size="small" />
                <el-input v-else v-model="scope.row.value" size="small" />
              </div>
            </template>
          </el-table-column>
        </el-table>
        <div class="container">
          <a class="btn btn-3">
            <el-icon color="white">
              <Plus />
            </el-icon>
          </a>
        </div>
      </el-main>
    </el-container>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { ElNotification } from "element-plus";
import { stringify } from "yaml";
import { trimDefault } from "../function/tools"
import { loadPB } from "../function/pb"
import { Search, Close } from '@element-plus/icons-vue'
import Logo from "./Logo.vue";
import Toolbar from "./toolbar/toolbar.vue";
import { add } from "lodash";
const addTag = ref("")
// Init
onMounted(async () => {
  await LoadPB()
  await LoadConfig();
});

// 菜单UI
const showDelete = ref("")
const visible = ref("")
function deleteHandle() {
  console.log("删除")
}
function addHandle(name: string) {
  visible.value = name
}
function cancelAdd() {
  visible.value = ""
}
function isVisible(curNmae: string) {
  return visible.value == curNmae
}
function hideHandle() {
  addTag.value = ""
}
function addConfig(service: string) {
  visible.value = ""
  for (let i = 1; ; i++) {
    if (service + "-" + "new_" + i in treeData.value) {
      continue
    }
    treeData.value[service]["new_" + i] = subTree(configData[service][addTag.value]);
    break
  }
}

// Proto
var configData = {}
var template = {}
var enums = ref()
const LoadPB = () => {
  window.go.main.App.LoadProto().then((resp: any) => {
    let pb = loadPB(resp.Data)
    template = genConfigTemplate(pb.pbObj)
    enums.value = genEnums(pb.enums)
  });
}
function genEnums(src: any) {
  let set = {}
  for (let kind in src) {
    let e = {}
    for (let key in src[kind]) {
      var keyToAny: any = key;
      if (isNaN(keyToAny)) {
        e[key] = src[kind][key]
      }
    }
    set[kind] = e
  }
  return set
}
function genConfigTemplate(data: any): any {
  let config = {}
  for (let field in data) {
    if (field.includes("_Config")) {
      config[field.split('_')[0].toLowerCase()] = data[field]
    }
  }
  return config
}
function genConfig(cache: any): any {
  let configs = {}
  console.log("缓存： ", cache)
  for (let svc in template) {
    if (cache != null && svc in cache[svc]) {
      for (let mode in cache) {
        configs[svc][mode] = setValue(cache[svc][mode], Object.assign({}, template[svc]))
      }
    } else {
      console.log("新服务: ", svc)
      configs[svc] = {}
      configs[svc]["template"] = Object.assign({}, template[svc])
    }
  }
  console.log(configs)
  return configs
}

// APIs
const LoadConfig = () => {
  window.go.main.App.LoadConfigCache().then((resp: any) => {
    // 根据缓存生产 config 数据
    configData = genConfig(JSON.parse(resp.Data))
    // 根据模板生成菜单项
    getMenu(configData);
    // 生成树形表格
    toTree(configData);
  });
};

const SaveConfig = () => {
  window.go.main.App.SaveConfig(JSON.stringify(toData(treeData.value))).then(
    (res: any) => {
      handleRes(res, "保存成功");
    }
  );
};

const GenerateConfig = async () => {
  let m = mode.value;
  if (m == "") {
    return ElNotification({
      title: "Notice",
      message: "请选择导出模式",
      type: "info",
      duration: 1000,
    });
  }
  let data = toData(treeData.value);
  let result = {};
  for (let i in data) {
    if (data[i][m] != undefined) {
      trimDefault(data[i][m]);
      result[i] = stringify(data[i][m]);
    }
  }
  SaveConfig()
  ElNotification({
    title: "Notice",
    message: JSON.stringify(result),
    type: "info",
  })
  await window.go.main.App.GenConfig(result).then((resp: any) =>
    handleRes(resp, "配置文件导出成功")
  );
};

const SyncDataTable = () => {
  window.go.main.App.SyncCsv().then((resp: any) => handleRes(resp, "数据表同步成功"));
};

// Data
interface Node {
  id: string;
  name: string;
  value: any;
  type: string;
  children: Node[];
}
const treeData = ref({}as {
  [k:string]:{
    [k:string]:any
  }
});
const showing = ref();

// Style
const height = ref();
height.value = document.body.clientHeight - 110 + "px";
window.onresize = () => {
  height.value = document.body.clientHeight - 110 + "px";
};

// 导出模式
const mode = ref("");
const options = [
  {
    label: "Dev",
    value: "dev",
  },
  {
    label: "Dev-Debug",
    value: "test",
  },
  {
    label: "QA",
    value: "qa",
  },
  {
    label: "国内生产",
    value: "pro-i",
  },
  {
    label: "国外生产",
    value: "pro-o",
  },
];

// 数据处理
var gID = 0;
function toTree(data: any) { //转换树形
  for (let service in data) {
    for (let mode in data[service]) {
      if (!(service in treeData.value)) {
        treeData.value[service] = {}
      }
      treeData.value[service][mode] = subTree(data[service][mode]);
    }
  }
}
function subTree(data: any) {
  let nodes = new Array<Node>();
  for (let index in data) {
    const node: Node = {
      id: gID.toString(),
      name: index,
      value: "",
      type: "",
      children: [],
    };
    gID++;
    if (data[index] instanceof Object && !Array.isArray(data[index])) {// 1.判断是否嵌套结构体
      if ("type" in data[index] && data[index].substring(0, 5) == "enum:") {    //判断是否为enums
        node.type = data[index]["type"]
        node.value = data[index]["value"]
      } else {
        node.children = subTree(data[index])
      }
    } else if (Array.isArray(data[index])) {  // 2.判断是否为数组
      node.type = "slice"
      node.value = data[index].toString()
    } else if (typeof data[index] == "string" && data[index].substring(0, 6) == "#enum#") {     // 3.常规字段
      node.type = "enum:" + data[index].substring(6)
      node.value = null
    } else {
      node.value = data[index]
    }
    nodes.push(node)
  }
  return nodes
}

function toData(data: any) {  //还原数据
  let configs = {};
  for (let service in data) {
    for (let mode in data) {
      if (configs[service] != undefined) {
        configs[service][mode] = subData(data[service][mode]);
      } else {
        configs[service] = {};
        configs[service][mode] = subData(data[service][mode]);
      }
    }
  }
  return configs;
}
function subData(data: Node[]) {
  let config: any = {};
  for (let i = 0; i < data.length; i++) {
    if (data[i].children.length == 0) {
      if (data[i].type == "slice") {
        if (data[i].value == "") {
          config[data[i].name.split("#")[0]] = [];
        } else {
          config[data[i].name.split("#")[0]] = data[i].value.split(",");
        }
      } else {
        config[data[i].name] = data[i].value;
      }
    } else {
      config[data[i].name] = subData(data[i].children);
    }
  }
  return config;
}

/**
 * @function 根据模板匹配缓存数据
 */
function setValue(src: any, dst: any) {
  for (let i in src) {
    if (i in dst) {
      // 1.判断是否为枚举
      if (typeof dst[i] == "string" && dst[i].substring(0, 6) == "#enum#") {
        dst[i] = {
          value: src[i],
          type: "enum:" + dst[i].substring(6)
        }
      }
      // 2.判断是否为嵌套结构
      if (
        src[i] instanceof Object &&
        dst[i] instanceof Object &&
        !Array.isArray(src[i])
      ) {
        setValue(src[i], dst[i]);
        continue
      }
      // 3.判断常规字段类型是否相同
      if (typeof src[i] == typeof dst[i]) {
        dst[i] = src[i];
      } else {
        console.log("mistake type: ", i);
      }
    }
  }
}

// utils
const handleSelect = (index:string, service:string, mode: string) => {
  showDelete.value = index;
  showing.value = treeData.value[service][mode];
};
const handleRes = (resp: string, successMsg: string) => {
  if (resp != "") {
    ElNotification({
      title: "Error",
      message: resp,
      type: "error",
      duration: 1000,
    });
  } else {
    ElNotification({
      title: "Success",
      message: successMsg,
      type: "success",
      duration: 1000,
    });
  }
};

// 菜单
interface menu {
  [key: string]: string[];
}
const menu = ref({} as menu);
function getMenu(data: any) {
  for (let key in data) {
    menu.value[key] = new Array<string>();
    for (let mode in data[key]) {
      menu.value[key].push(mode);
    }
  }
}

// Style
const expendedRow = ref([] as string[]);
const handleExpend = (row: any, isExpend: boolean) => {
  if (isExpend) {
    expendedRow.value.push(row.id);
  } else {
    remove(expendedRow.value, row.id);
  }
};
function remove(dst: string[], key: string) {
  var index = dst.indexOf(key);
  if (index > -1) {
    dst.splice(index, 1);
  }
}
</script>

<style scoped>
.aside {
  background: var(--color-basic-white);
  margin: 16px 0px 0px 16px;
  height: calc(100vh - 100px);
  box-shadow: var(--el-box-shadow-light);
  border-radius: var(--el-border-radius-base);
  overflow: hidden;
  transition: width 0.3 ease;
}

.header {
  display: flex;
  align-items: center;
}

.menu {
  border: 0;
  height: 100%;
}

.add-line {
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #a7d0fc;
}

.delete-buttom {
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  z-index: 10000;
  color: white;
  width: 20px;
  height: 50px;
  margin-left: auto;
  background-color: rgba(128, 128, 128, 0.233);
  transition: all 150ms linear;
}

.delete-buttom:hover {
  color: white;
  background-color: #e02c26;
  transition: all 250ms linear;
}

.add-confirm-box {
  display: flex;
  justify-content: center;
  align-items: center;
}
</style>
