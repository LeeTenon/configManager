<template>
  <div class="common-layout">
    <el-container>
      <el-header class="header" style="--wails-draggable: drag">
        <div>
          <el-select v-model="mode" class="select" placeholder="请选择导出模式" style="width: 150px; margin-right: 10px">
            <el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value" />
          </el-select>
          <el-button v-if="mode == ''" type="primary" disabled>保持并生成配置文件</el-button>
          <el-button v-else type="primary" @click="SaveConfig()">保持并生成配置文件</el-button>
          <!-- <el-button type="primary" @click="SaveConfig()">保存配置</el-button> -->
          <el-button type="primary" @click="SyncDataTable()">同步数据表</el-button>
        </div>
        <Toolbar />
      </el-header>
    </el-container>
    <el-container>
      <el-aside width="200px" class="aside">
        <Logo />
        <el-menu class="menu">
          <el-sub-menu v-for="(configs, service) in treeData" :index="service">
            <template #title>
              <div class="service-tag-box">
                <span>S</span>
              </div>
              <h4>{{ service }}</h4>
            </template>
            <el-menu-item v-for="(config, mode) in configs" :index="service + '-' + mode"
              @click="handleSelect(service + '-' + mode,service as string,mode as string)">
              <template #title>
                <div style="display: flex;  align-items: center;">
                  <div class="config-tag-box">
                    <span>S</span>
                  </div>
                  <span>{{ mode }}</span>
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
              </template>
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
        <el-table :data="showing" style="width: 100%" :max-height="height" row-key="id" border size="small"
          :expand-row-keys="expendedRow" @expand-change="handleExpend">
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
              <div v-if="isNil(scope.row.children)" style="display: flex; align-items: center">
                <div v-if="isNil(scope.row.common) || scope.row.force" class="value-box">
                  <el-select v-if="scope.row.type == 'enum'" class="common-value-input" placeholder="Select"
                    size="small" clearable v-model="scope.row.value">
                    <el-option v-for="(value,key) in enums[scope.row.spec]" :key="value" :label="key" :value="key" />
                  </el-select>
                  <el-select v-else-if="scope.row.type == 'bool'" class="common-value-input" placeholder="Select"
                    size="small" clearable v-model="scope.row.value">
                    <el-option v-for="value in boolEnum" :key="value.value" :label="value.lable" :value="value.value" />
                  </el-select>
                  <el-input-number v-else-if="scope.row.type == 'number'" v-model="scope.row.value" size="small"
                    class="common-value-input" />
                  <el-input v-else v-model="scope.row.value" size="small" class="common-value-input" />
                  <el-button v-if="!isUndefined(scope.row.common)" @click="scope.row.force = !scope.row.force"
                    size="small" style="margin-left: 15px;">
                    使用通用配置</el-button>
                </div>
                <div v-else class="value-box">
                  <el-select v-if="scope.row.type == 'enum'" class="common-value-input" placeholder="Select"
                    size="small" clearable v-model="scope.row.common.value" disabled>
                    <el-option v-for="(value,key) in enums[scope.row.spec]" :key="value" :label="key" :value="key" />
                  </el-select>
                  <el-select v-else-if="scope.row.type == 'bool'" class="common-value-input" placeholder="Select"
                    size="small" clearable v-model="scope.row.common.value" disabled>
                    <el-option v-for="value in boolEnum" :key="value.value" :label="value.lable" :value="value.value" />
                  </el-select>
                  <el-input-number v-else-if="scope.row.type == 'number'" v-model="scope.row.common.value" size="small"
                    disabled class="common-value-input" />
                  <el-input v-else v-model="scope.row.common.value" size="small" disabled />
                  <el-button @click="scope.row.force = !scope.row.force" size="small" style="margin-left: 15px;">修改
                  </el-button>
                </div>
              </div>
            </template>
          </el-table-column>
        </el-table>
      </el-main>
    </el-container>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { ElNotification } from "element-plus";
import { stringify } from "yaml";
import { loadPB } from "../function/pb"
import { Close } from '@element-plus/icons-vue'
import { isArray, isUndefined } from "lodash";
import Logo from "./Logo.vue";
import Toolbar from "./toolbar/toolbar.vue";

const commonConfig = 'CommonConfig'

const boolEnum = [
  {
    'lable': 'true',
    'value': true
  },
  {
    'lable': 'false',
    'value': false
  }
]

// Init
onMounted(async () => {
  await LoadPB()
  await LoadConfig();
});

// #region Data
class TreeNode {
  id: string;
  name: string;
  value: any;
  type: string;
  spec: string;
  common: TreeNode | undefined
  force: boolean
  children: TreeNode[];
  constructor(id: string, name: string) {
    this.id = id
    this.name = name
  }
}

interface Tree {
  [k: string]: {
    [k: string]: TreeNode[]
  }
}
const treeData = ref({} as Tree)
const showing = ref();
// #endregion

// #region APIs
const LoadConfig = () => {
  window.go.main.App.LoadConfigCache().then((resp: any) => {
    // 读取缓存树
    treeData.value = loadCacheTree(JSON.parse(resp.Data))
  });
};
// 保存
const SaveConfig = async () => {
  let data = JSON.parse(JSON.stringify(treeData.value)) as Tree
  trimAllCommon(data)
  await window.go.main.App.SaveConfig(JSON.stringify(data, null, 2)).then((resp) => {
    if (resp.Error != '') {
      showError(resp.Error)
    }
  })
  // 生成配置文件
  let yamlData = genYaml(mode.value);
  await window.go.main.App.GenConfig(yamlData).then((resp: any) => {
    if (resp.Error != '') {
      showError(resp.Error)
    } else {
      showSuccess('保存并生成配置文件成功')
    }
  })
}
const SyncDataTable = () => {

}
// #endregion

// #region Proto->Data
var serviceTreeTemplate = {}
var commonTreeTemplate = {}
var enums = ref()

const LoadPB = () => {
  window.go.main.App.LoadProto().then((resp: any) => {
    let pb = loadPB(resp.Data)
    serviceTreeTemplate = genTreeTemplate(genConfigTemplate(pb.pbObj))
    commonTreeTemplate = objToTree(pb.pbCommonObj)
    serviceTreeTemplate[commonConfig] = commonTreeTemplate
    enums.value = genEnums(pb.enums)
  });
}

function genTreeTemplate(objTemplate: any): { [k: string]: TreeNode[] } {
  let template = {}
  for (let service in objTemplate) {
    template[service] = objToTree(objTemplate[service])
  }
  return template
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
// #endregion

// #region 数据处理
function loadCacheTree(cache: Tree): Tree {
  let tree = {}
  for (let service in serviceTreeTemplate) {
    tree[service] = {}
    if (!isNil(cache) && service in cache) {
      for (let mode in cache[service]) {
        let data = JSON.parse(JSON.stringify(serviceTreeTemplate[service])) as TreeNode[]
        setValue(cache[service][mode], data)
        tree[service][mode] = data
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
      }
    }
  }
  // 绑定common数据
  if (commonConfig in tree) {
    for (let m in tree[commonConfig]) {
      for (let s in tree) {
        if (s != commonConfig && m in tree[s]) {
          setCommon(tree[commonConfig][m], tree[s][m])
        }
      }
    }
  }
  return tree
}

function genYaml(mode: string): { [k: string]: string } {  //还原数据
  let configs = {};
  for (let service in treeData.value) {
    if (service != commonConfig) {
      if (mode in treeData.value[service]) {
        configs[service] = stringify(treeToObj(treeData.value[service][mode]))
        if (configs[service] == undefined) {
          configs[service] = ''
        }
      }
    }
  }
  return configs;
}

function objToTree(data: any): TreeNode[] {
  return DoObjToTree(data, '')
}
function DoObjToTree(data: any, parentStr: string): TreeNode[] {
  let nodes = new Array<TreeNode>();
  for (let field in data) {
    let id = parentStr + '.' + field
    const node = new TreeNode(id, field);
    if ("#type#" in data[field] && "#value#" in data[field]) {// 1.叶子节点
      node.force = false
      node.type = data[field]["#type#"]
      switch (node.type) {
        case "enum":
          node.value = data[field]["#value#"]
          node.spec = data[field]["spec"]
        default:
          node.value = data[field]["#value#"]
          break;
      }
    } else {// 2.嵌套结构
      node.children = DoObjToTree(data[field], id)
    }
    nodes.push(node)
  }
  return nodes
}
function treeToObj(data: TreeNode[]): any {
  let config = {};
  for (let i = 0; i < data.length; i++) {
    let node = data[i]
    let value: any
    if (isNil(node.children)) { //  叶子节点
      if (!node.force && !isNil(node.common)) { //使用common配置
        value = node.common!.value
      } else {
        value = node.value
      }
      if (!isZero(value)) {
        if (node.type == "slice") {//1.数组        
          let s = value.split(",");
          if (node.spec == "number") {
            let numArr = new Array<number>
            for (let i = 0; i < s.length; i++) {
              numArr.push(parseInt(s[i]))
            }
            config[node.name] = numArr
          } else {
            config[node.name] = value.split(",");
          }
        } else {//2.基本类型
          config[node.name] = value;
        }
      }
    } else {//3.嵌套结构体
      let sub = treeToObj(data[i].children)
      if (!isNil(sub)) {
        if (node.name[0] == '_') {
          for (let f in sub) {
            config[f] = sub[f]
          }
        } else {
          config[node.name] = sub
        }
      }
    }
  }

  if (Object.keys(config).length > 0) {
    return config
  }
}

/**
 * @function 根据模板匹配缓存树数据
 */
function setValue(src: TreeNode[], dst: TreeNode[]) {
  for (let i = 0; i < src.length; i++) {
    let field = dst.find((item) => {
      return item.id == src[i].id
    })
    if (field == undefined) {
      return
    }
    if (!isNil(src[i].children)) {  //嵌套结构体
      setValue(src[i].children, field.children)
    } else if (isNodeEuql(field, src[i])) {  //叶子节点
      field.value = src[i].value
      field.force = src[i].force
    }
  }
}
/**
 * @function 绑定common配置
 */
function setCommon(src: TreeNode[], dst: TreeNode[]) {
  for (let i = 0; i < src.length; i++) {
    let field = dst.find((item) => {
      return item.id == src[i].id
    })
    if (field == undefined) {
      continue
    }
    if (!isNil(field.children) && !isNil(src[i].children)) {  //嵌套结构体
      setCommon(src[i].children, field.children)
    } else {  //叶子节点
      field.common = src[i]
    }
  }
}

/**
 * @function 解绑common配置
 */
function trimCommon(nodes: TreeNode[]) {
  for (let i = 0; i < nodes.length; i++) {
    if (isNil(nodes[i].children)) {  //叶子节点
      // delete nodes[i].common 
      nodes[i].common = undefined
    } else {                        //嵌套结构体
      trimCommon(nodes[i].children)
    }
  }
}
function trimAllCommon(tree: Tree) {
  for (let service in tree) {
    for (let mode in tree[service]) {
      trimCommon(tree[service][mode])
    }
  }
}
// #endregion

// #region 菜单UI
const addTag = ref("")
const showDelete = ref("")
const visible = ref("")
const newConfigName = ref("")
function handleSelect(index: string, service: string, mode: string) {
  showDelete.value = index;
  showing.value = treeData.value[service][mode];
}

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
  // 边界判断
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
  // 追加配置项
  if (addTag.value == "empty") {
    treeData.value[service][name] = JSON.parse(JSON.stringify(serviceTreeTemplate[service]));
  } else {
    treeData.value[service][name] = JSON.parse(JSON.stringify(treeData.value[service][addTag.value]));
    trimCommon(treeData.value[service][name])
  }
  // 绑定common配置
  if (service == commonConfig) {
    for (let s in treeData.value) {
      if (name in treeData.value[s] && s != service) {
        setCommon(treeData.value[service][name], treeData.value[s][name])
      }
    }
  } else {
    if (name in treeData.value[commonConfig]) {
      setCommon(treeData.value[commonConfig][name], treeData.value[service][name])
    }
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
function isZero(val: any) {
  return (val == 0 || val == '' || val == null || val == false || val == undefined || (isArray(val) && val.length == 0))
}
function isNodeEuql(a: TreeNode, b: TreeNode) {
  return (a.id == b.id && a.type == b.type)
}
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
const showError = (msg: string) => {
  return ElNotification({
    title: "Error",
    message: msg,
    type: "error",
    duration: 1000,
  });
}
const showSuccess = (msg: string) => {
  return ElNotification({
    title: "Success",
    message: msg,
    type: "success",
    duration: 1000,
  });
}

function isNil(val: any) {
  return val == null || val == undefined
}
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
  transition: all 150ms linear;
}

.add-line:hover {
  background-color: #66abf5;
  transition: all 250ms linear;
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
  margin-right: -20px;
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

.value-box {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
}

.common-value-input {
  flex: 1;
}

.service-tag-box {
  height: 20px;
  width: 20px;
  background-color: #66abf5;
  display: inline-block;
  text-align: center;
  line-height: 20px;
  margin-right: 10px;
  border-radius: 3px;
  color: white;
  font-weight: 1000;
}

.config-tag-box {
  height: 20px;
  width: 20px;
  background-color: rgb(234, 234, 111);
  display: inline-block;
  text-align: center;
  line-height: 20px;
  margin-right: 10px;
  border-radius: 3px;
  color: white;
  font-weight: 1000;
}
</style>
