<template>
  <div class="common-layout">
    <el-container>
      <el-header class="header" style="--wails-draggable: drag">
        <!-- <div>
          <el-select v-model="mode" class="select" placeholder="请选择导出模式" style="width: 150px; margin-right: 10px">
            <el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value" />
          </el-select>
          <el-button v-if="mode == ''" type="primary" disabled>保存并生成配置</el-button>
          <el-button v-else type="primary" @click="SaveConfig()">保存并生成配置</el-button>
          <el-button type="primary" @click="SyncDataTable()">同步数据表</el-button>
          <el-popover placement="bottom" :width="400" trigger="click" @show="checkBanch" :visible="banchVisible">
            <template #reference>
              <el-button @click="banchVisible = !banchVisible">批量创建配置</el-button>
            </template>
            <div class="add-confirm-box" style="padding:10px">
              <el-form-item label="(逗号分隔)配置名：" label-width="140px">
                <el-input v-model="banchTarget" />
              </el-form-item>
              <el-form-item label="复制模板：" label-width="140px">
                <el-select placeholder="Select" clearable v-model="addTag" style="width: 100%;">
                  <el-option key="empty" label="空模板" value="empty" />
                  <el-option v-for="item in canBanchTemplate" :key="item.value" :label="item.lable"
                    :value="item.value" />
                </el-select>
              </el-form-item>
              <div style=" align-self: center;">
                <el-button v-if="banchTarget == '' || addTag == ''" type="primary" disabled>确定</el-button>
                <el-button v-else type="primary" @click="banchAddConfig()">确定</el-button>
                <el-button type="primary" @click="cancelAdd">取消</el-button>
              </div>
            </div>
          </el-popover>
        </div> -->
        <Toolbar />
      </el-header>
    </el-container>
    <el-container>
      <el-aside width="200px" class="aside">
        <Logo />
        <el-scrollbar style="height: calc(100% - 50px);" always>
          <el-menu class="menu">
            <el-sub-menu v-for="(configs, service) in configTree" :index="service">
              <template #title>
                <div class="service-tag-box">
                  <span>S</span>
                </div>
                <h4>{{ service }}</h4>
              </template>
              <el-menu-item v-for="(config, mode) in configs" :index="service + '-' + mode"
                @click="handleSelect(service + '-' + mode, service as string, mode as string)">
                <template #title>
                  <div style="display: flex;  align-items: center;">
                    <div class="config-tag-box">
                      <span>M</span>
                    </div>
                    <span>{{ mode }}</span>
                  </div>
                  <el-popconfirm v-if="showDelete == service + '-' + mode" confirm-button-text="Yes"
                    cancel-button-text="No" title="确认删除?" @confirm="confirmDelete(service, mode)">
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
                      @click="addConfig(service, newConfigName)">确定</el-button>
                    <el-button v-else type="primary" @click="addConfig(service, newConfigName)">确定</el-button>
                    <el-button type="primary" @click="cancelAdd">取消</el-button>
                  </div>
                </div>
              </el-popover>
            </el-sub-menu>
          </el-menu>
        </el-scrollbar>
      </el-aside>
      <!--数据主体-->
      <el-main>
        <Table :data="showing"></Table>
      </el-main>
    </el-container>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, reactive } from "vue";
import { ElNotification } from "element-plus";
import { stringify } from "yaml";
import { loadPB } from "../../function/pb"
import { Close } from '@element-plus/icons-vue'
import { isArray } from "lodash";

import Logo from "./components/Logo.vue";
import Toolbar from "./components/toolbar.vue";
import Table from './components/table.vue'

// 常量
const commonConfig = 'CommonConfig'

// 全局变量
var templates: { [k: string]: TreeNode[] } = {}
var configTree = ref<Tree>()
const showing = ref();

// #region Init
onMounted(async () => {
  await LoadPB()
  await LoadCache();
});

const LoadPB = () => {
  window.go.main.App.LoadProto().then((resp: any) => {
    let configs = loadPB(resp.Data)
    for (let i in configs) {
      templates[i] = obj2Tree(configs[i])
    }
  });
}

const LoadCache = () => {
  window.go.main.App.LoadConfigCache().then((resp: any) => {
    // 读取缓存树
    configTree.value = loadCacheTree(JSON.parse(resp.Data))
    console.log(configTree.value)
  });
};
// #endregion

// #region APIs
// 保存
const SaveConfig = async () => {
  let data = JSON.parse(JSON.stringify(configTree.value)) as Tree
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
const SyncDataTable = async () => {
  await window.go.main.App.SyncData().then((resp: any) => {
    if (resp.Error != '') {
      showError(resp.Error)
    } else {
      showSuccess('同步数据表成功')
    }
  })
}
// #endregion

// #region 数据处理
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
    this.force = false
  }
}

interface Tree {
  [serviceName: string]: {
    [modeName: string]: TreeNode[]
  }
}

function obj2Tree(obj: Object): TreeNode[] {
  return transform(obj, '')
}

function transform(data: any, parentField: string): TreeNode[] {
  let nodes = new Array<TreeNode>();
  for (let field in data) {
    let id = parentField + '.' + field
    const node = new TreeNode(id, field);
    if ('$type' in data[field] && '$spec' in data[field]) {// 1.叶子节点
      node.type = data[field]['$type']
      node.spec = data[field]['$spec']
      if (data[field]['$spec'] == 'slice') {
        node.value = []
      }
    } else {// 2.嵌套结构
      node.children = transform(data[field], id)
    }
    nodes.push(node)
  }
  return nodes
}

function loadCacheTree(cache: Tree): Tree {
  let tree = {}
  for (let service in templates) {
    tree[service] = {}
    if (!isNil(cache) && service in cache) {
      for (let mode in cache[service]) {
        let data = JSON.parse(JSON.stringify(templates[service])) as TreeNode[]
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
  for (let service in configTree.value) {
    if (service != commonConfig) {
      if (mode in configTree.value[service]) {
        configs[service] = stringify(treeToObj(configTree.value[service][mode]))
        if (configs[service] == undefined) {
          configs[service] = ''
        }
      }
    }
  }
  return configs;
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
      continue
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
  showing.value = configTree.value[service][mode];
}

function deleteHandle(service: any, mode: any) {
  delete configTree.value[service][mode]
}

function confirmDelete(service: any, mode: any) {
  delete configTree.value[service][mode]
}

function addHandle(name: string) {
  newConfigName.value = ""
  visible.value = name
}

function cancelAdd() {
  banchTarget.value = ''
  addTag.value = ""
  banchVisible.value = false
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
  if (name in configTree.value[service]) {
    return ElNotification({
      title: "Error",
      message: "配置文件命名重复",
      type: "error",
      duration: 1000,
    });
  }
  // 追加配置项
  if (addTag.value == "empty") {
    configTree.value[service][name] = JSON.parse(JSON.stringify(templates[service]));
  } else {
    configTree.value[service][name] = JSON.parse(JSON.stringify(configTree.value[service][addTag.value]));
    trimCommon(configTree.value[service][name])
  }
  // 绑定common配置
  if (service == commonConfig) {
    for (let s in configTree.value) {
      if (name in configTree.value[s] && s != service) {
        setCommon(configTree.value[service][name], configTree.value[s][name])
      }
    }
  } else {
    if (name in configTree.value[commonConfig]) {
      setCommon(configTree.value[commonConfig][name], configTree.value[service][name])
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
function isZero(val: any) {
  return (val == 0 || val == '' || val == null || val == false || val == undefined || (isArray(val) && val.length == 0))
}
function isNodeEuql(a: TreeNode, b: TreeNode) {
  return (a.id == b.id && a.type == b.type)
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
