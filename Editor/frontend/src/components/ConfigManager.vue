<template>
  <div class="common-layout">
    <el-container>
      <el-header class="header" style="--wails-draggable: drag">
        <div>
          <el-select v-model="mode" class="select" placeholder="请选择导出模式" style="width: 150px; margin-right: 10px">
            <el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value" />
          </el-select>
          <el-button v-if="mode == ''" type="primary" disabled>生成配置</el-button>
          <el-button v-else type="primary" @click="GenerateConfig()">生成配置</el-button>
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
            <el-menu-item v-for="(config, mode) in configs" :index="service + '-' + mode"
              @click="handleSelect(service + '-' + mode,service as string,mode as string)">
              <div style="display: flex;width:calc(100% - 30px);">
                <div>
                  {{ mode }}
                </div>
                <el-popconfirm v-if="showDelete == service + '-' + mode" confirm-button-text="Yes"
                  cancel-button-text="No" title="确认删除?" @confirm="confirmDelete(service,mode)" @cancel="cancelDelete">
                  <template #reference>
                    <div class="delete-buttom">
                      <el-icon style="margin-left: 5px;">
                        <Close />
                      </el-icon>
                    </div>
                  </template>
                </el-popconfirm>
              </div>
            </el-menu-item>
            <el-popover placement="right" :width="300" trigger="click" :visible="isVisible(service as string)"
              @before-leave="hideHandle">
              <template #reference>
                <div class="add-line" @click="addHandle(service as string)">
                  <el-icon color="white">
                    <Plus />
                  </el-icon>
                </div>
              </template>
              <div class="add-confirm-box">
                <el-form-item label="配置名：" label-width="100px">
                  <el-input v-model="newConfigName" />
                </el-form-item>
                <el-form-item label="复制模板：" label-width="100px">
                  <el-select placeholder="Select" clearable v-model="addTag">
                    <el-option key="empty" label="空模板" value="empty" />
                    <el-option v-for="(config, mode) in configs" :key="mode" :label="mode" :value="mode" />
                  </el-select>
                </el-form-item>
                <div style=" align-self: center; ;">
                  <el-button v-if="newConfigName == '' || addTag == ''" type="primary" disabled
                    @click="addConfig(service,newConfigName)">确定</el-button>
                  <el-button v-else type="primary" @click="addConfig(service,newConfigName)">确定</el-button>
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
                <el-select v-if="scope.row.type == 'enum'" class="m-2" placeholder="Select" size="small" clearable
                  v-model="scope.row.value">
                  <el-option v-for="(value,key) in enums[scope.row.spec]" :key="value" :label="key" :value="key" />
                </el-select>
                <el-input-number v-else-if="scope.row.type == 'number'" v-model="scope.row.value" size="small" />
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
import { Close } from '@element-plus/icons-vue'
import { isArray, isObject } from "lodash";
import Logo from "./Logo.vue";
import Toolbar from "./toolbar/toolbar.vue";

// Init
onMounted(async () => {
  await LoadPB()
  await LoadConfig();
});
function handleSelectCommon() {
  showDelete.value = ''
  showing.value = commonTree
}

// #region Data
interface Node {
  id: string;
  name: string;
  value: any;
  type: string;
  spec: string;
  children: Node[];
}
const treeData = ref({} as {
  [k: string]: {
    [k: string]: any
  }
});
const showing = ref();
// #endregion

// #region APIs
const LoadConfig = () => {
  window.go.main.App.LoadConfigCache().then((resp: any) => {
    // 根据缓存生产 config 数据
    configData = genConfig(JSON.parse(resp.Data))
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
  let data = toData(treeData.value);
  let result = {};
  for (let i in data) {
    if (data[i][m] != undefined) {
      trimDefault(data[i][m]);
      result[i] = stringify(data[i][m]);
    }
  }
  console.log(result)
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
// #endregion

// #region Proto
var commonTree = {}
var commonConfigTemplate = {}
var configData = {}
var template = {}
var enums = ref()
const LoadPB = () => {
  window.go.main.App.LoadProto().then((resp: any) => {
    let pb = loadPB(resp.Data)
    template = genConfigTemplate(pb.pbObj)
    enums.value = genEnums(pb.enums)
    commonConfigTemplate = pb.pbCommonObj
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
  // 生成common配置
  let common = {}
  if (cache != null && '_common_' in cache) {
    for (let mode in cache['_common_']) {
      let tmp = JSON.parse(JSON.stringify(commonConfigTemplate))
      setValue(cache['_common_'], tmp)
      common[mode] = tmp
    }
    delete cache['_common_']
  }
  // 生成服务配置
  let configs = {}
  for (let svc in template) {
    configs[svc] = {}
    if (cache != null && svc in cache) {
      for (let mode in cache[svc]) {
        //加入导出mode列表
        let isValid = false
        for (let i = 0; i < options.value.length; i++) {
          if (options.value[i]["label"] == mode) {
            isValid = true
            break
          }
        }
        if (!isValid) {
          options.value.push(
            {
              value: mode,
              label: mode,
            },
          )
        }
        let tmp = JSON.parse(JSON.stringify(template[svc]))
        setValue(cache[svc][mode], tmp)
        console.log(commonConfigTemplate, tmp)
        setCommon(commonConfigTemplate, tmp)
        configs[svc][mode] = tmp
      }
    }
  }
  configs['CommonConfig'] = common
  return configs
}
// #endregion

// #region 数据处理
var gID = 0;
function toTree(data: any) { //转换树形
  for (let service in data) {
    treeData.value[service] = {}
    for (let mode in data[service]) {
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
      spec: "",
      children: [],
    };
    gID++
    if ("#type#" in data[index] && "#value#" in data[index]) {// 1.叶子节点
      node.type = data[index]["#type#"]
      switch (node.type) {
        case "slice":
          node.value = data[index]["#value#"].toString()
          break;
        case "enum":
          node.value = data[index]["#value#"]
          node.spec = data[index]["spec"]
        default:
          node.value = data[index]["#value#"]
          break;
      }
    } else {// 2.嵌套结构
      node.children = subTree(data[index])
    }
    nodes.push(node)
  }
  return nodes
}

function toData(data: any): any {  //还原数据
  let configs = {};
  for (let service in data) {
    for (let mode in data[service]) {
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
function subData(data: Node[]): any {
  let config: any = {};
  for (let i = 0; i < data.length; i++) {
    if (data[i].children.length == 0) {
      if (data[i].type == "slice") {//1.数组
        if (data[i].value == "") {
          config[data[i].name] = [];
          continue
        }
        let s = data[i].value.split(",");
        if (data[i].spec == "number") {
          let numArr = []
          for (let i = 0; i < s.length; i++) {
            numArr.push(parseInt(s[i]))
          }
          config[data[i].name] = numArr
        } else {
          config[data[i].name] = data[i].value.split(",");
        }
      } else {//2.基本类型
        config[data[i].name] = data[i].value;
      }
    } else {//3.嵌套结构体
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
      // 1.判断是否为常规字段类型（叶子节点）
      console.log(dst)
      if ("#type#" in dst[i] && "#value#" in dst[i]) {
        dst[i]["#value#"] = src[i]
        continue
      } else if (isObject(src[i]) && isObject(dst[i])) {
        setValue(src[i], dst[i]);
        continue
      }
      console.log("失效类型：", i)
    }
  }
}

/**
 * @function 读取common配置
 */
function setCommon(common: any, dst: any) {
  for (let i in common) {
    if (i in dst) {
      // 1.判断是否为常规字段类型（叶子节点）
      if ("#type#" in dst[i] && "#value#" in dst[i]) {
        if (isZero(dst[i]["#value#"])) {
          dst[i]["#value#"] = common[i]
        }
        continue
      } else if (isObject(common[i]) && isObject(dst[i])) {
        setCommon(common[i], dst[i]);
        continue
      }
      console.log("失效类型：", i)
    }
  }
}
/**
 * @function 剔除common配置
 */
function trimCommon(common: any, dst: any) {
  for (let i in common) {
    if (i in dst) {
      // 1.判断是否为常规字段类型（叶子节点）
      if ("#type#" in dst[i] && "#value#" in dst[i] && !(isObject(common[i]) && !isArray(common[i]))) {
        if (dst[i]["#type#"] == 'slice') {
          if (dst[i]["#value#"].toString() == dst[i].toString()) {
            dst[i]["#value#"] = []
          }
        } else {
          if (dst[i]["#value#"] == common[i]) {
            dst[i]["#value#"] = null
          }
        }
        continue
      } else if (isObject(common[i]) && isObject(dst[i])) {
        trimCommon(common[i], dst[i]);
        continue
      }
      console.log("失效类型：", i)
    }
  }
}
function isZero(val: any) {
  return (val == 0 || val == '' || val == null || val == undefined || (isArray(val) && val.length == 0))
}
// #endregion

// #region 菜单UI
const addTag = ref("")
const showDelete = ref("")
const visible = ref("")
const newConfigName = ref("")
function deleteHandle(service: any, mode: any) {
  delete treeData.value[service][mode]
}
function confirmDelete(service: any, mode: any) {
  delete treeData.value[service][mode]
}
function cancelDelete() {

}
function addHandle(name: string) {
  newConfigName.value = ""
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
function addConfig(service: any, name: any) {
  visible.value = ""
  if (name == "empty") {
    return ElNotification({
      title: "Error",
      message: "empty 为保留字",
      type: "error",
      duration: 1000,
    });
  }
  if (name in treeData.value[service]) {
    return ElNotification({
      title: "Error",
      message: "配置文件命名重复",
      type: "error",
      duration: 1000,
    });
  }

  if (addTag.value == "empty") {
    console.log(service)
    if (service == 'CommonConfig') {
      treeData.value[service][name] = subTree(commonConfigTemplate)
    } else {
      treeData.value[service][name] = subTree(template[service])
    }
  } else {
    treeData.value[service][name] = JSON.parse(JSON.stringify(treeData.value[service][addTag.value]));
  }
  //加入导出mode列表
  let isValid = false
  for (let i = 0; i < options.value.length; i++) {
    if (options.value[i]["label"] == name) {
      isValid = true
      break
    }
  }
  if (!isValid) {
    options.value.push(
      {
        value: name,
        label: name,
      },
    )
  }
}
// #endregion

// #region 导出模式
interface ModeOption {
  label: string
  value: string
}
const mode = ref("");
const options = ref([] as ModeOption[])
// #endregion

// #region utils
const handleSelect = (index: string, service: string, mode: string) => {
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
// #endregion

// #region Style
const height = ref();
height.value = document.body.clientHeight - 110 + "px";
window.onresize = () => {
  height.value = document.body.clientHeight - 110 + "px";
};


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
// #endregion
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
  cursor: pointer;
}

.add-line:hover {
  background-color: #66abf5;
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
  flex-direction: column;
  justify-content: center;
}
</style>
