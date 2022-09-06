<template>
  <div class="common-layout">
    <el-container>
      <el-header class="header" style="--wails-draggable: drag">
        <div>
          <el-select v-model="mode" class="select" placeholder="请选择导出模式" style="width: 150px; margin-right: 10px">
            <el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value" />
          </el-select>
          <el-button type="primary" @click="GenerateConfig(mode)">生成配置</el-button>
          <el-button type="primary" @click="SaveConfig()">保存配置</el-button>
          <el-button type="primary" @click="SyncCsv()">同步csv表</el-button>
        </div>
        <Toolbar />
      </el-header>
    </el-container>
    <el-container>
      <el-aside width="150px" class="aside">
        <Logo />
        <el-menu class="menu">
          <el-sub-menu v-for="(modes, service) in menu" :index="service">
            <template #title>
              <h4>{{ service }}</h4>
            </template>
            <el-menu-item v-for="(mode, i) in modes" :index="service + '-' + mode" @click="handleSelect">
              {{ mode }}
            </el-menu-item>
          </el-sub-menu>
        </el-menu>
      </el-aside>
      <!--数据主体-->
      <el-main>
        <el-table :data="showing" style="width: 100%;" :max-height="height" row-key="id" border>
          <el-table-column prop="name" label="配置项" style="width: 50%">
            <template #default="scope">
              <div v-if="scope.row.name.split('#').length>1" style="display: flex; align-items: center">
                <span>{{scope.row.name.split('#')[0]}}</span>
                <el-tag type="info" style="margin-left: 10px;">Slice</el-tag>
              </div>
              <span v-else>{{scope.row.name}}</span>
            </template>
          </el-table-column>
          <el-table-column prop="value" label="值" style="width: 50%">
            <template #default="scope">
              <div style="display: flex; align-items: center">
                <el-input-number v-if="typeof scope.row.value == 'number'" v-model="scope.row.value"/>
                <el-input v-else v-model="scope.row.value" placeholder="Please input" />
              </div>
            </template>
          </el-table-column>
        </el-table>
      </el-main>
    </el-container>
  </div>
</template>

<script setup lang="ts">
import { onBeforeMount, ref } from "vue";
import { ElNotification } from "element-plus";
import Logo from "./Logo.vue";
import Toolbar from "./toolbar/toolbar.vue";

// const hack = {
//   "challenge": {
//     "dev": {
//       "Name": "rpc.challenge",
//       "Mode": "dev",
//       "Redis": {
//         "Host": "redis:6379"
//       }
//     },
//     "pro": {
//       "Name": "rpc.challenge",
//       "Mode": "pro",
//       "Redis": {
//         "Host": "redis:6379"
//       }
//     }
//   }
// }

// Init
onBeforeMount(() => {
  LoadConfig();
});

// APIs
const LoadConfig = async () => {
  let template: any
  let cache: any
  await window.go.main.App.LoadConfigTemplate().then((resp: any) => {
    template = JSON.parse(resp)
  });
  await window.go.main.App.LoadConfigCache().then((resp: any) => {
    cache = JSON.parse(resp)
  });
  console.log(cache)
  setValue(cache, template)
  getMenu(template);
  toTree(template);
};
const SaveConfig = () => {
  console.log(JSON.stringify(toData(treeData.value)))
  window.go.main.App.SaveConfig(JSON.stringify(toData(treeData.value))).then((res: any) => {
    handleRes(res);
  });
};
const GenerateConfig = async (mode: string) => {
  await window.go.main.App.SaveConfig(toData(treeData.value));
  window.go.main.App.GenConfig(mode).then((response: any) =>
    ElNotification({
      title: "Success",
      message: "配置文件导出成功",
      type: "success",
    })
  );
};
const SyncCsv = () => {
  window.go.main.App.SyncCsv().then((response: any) =>
    ElNotification({
      title: "Success",
      message: "csv同步成功",
      type: "success",
    })
  );
};

// Data
interface tree {
  [index: string]: Node[]
}
interface Node {
  id: number
  name: string
  value: any
  children: Node[]
}
const treeData = ref({} as tree);
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
    label: "Dev-开发",
    value: "dev",
  },
  {
    label: "Pro-生产",
    value: "pro",
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

let gID = 0
// 数据处理
function toTree(data: any) {  //转换树形
  for (let index in data) {
    for (let i in data[index]) {
      treeData.value[index + "-" + i] = subTree(data[index][i]);
    }
  }
}
function subTree(data: any) {
  let nodes = new Array<Node>()
  for (let index in data) {
    const node: Node = {
      id: gID,
      name: index,
      value: "",
      children: []
    }
    gID++

    if (Array.isArray(data[index])) {
      console.log("find array:", index)
      node.name = node.name + "#array"
      let a = data[index]
      node.value = a.toString()
    } else if (data[index] instanceof Object) {
      node.children = subTree(data[index])
    } else {
      node.value = data[index]
    }
    nodes.push(node)
  }
  return nodes
}
function toData(data: tree) { //还原数据
  let configs = {};
  for (let index in data) {
    let service = index.substring(0, index.indexOf("-"));
    let mode = index.substring(index.indexOf("-") + 1);
    if (configs[service] != undefined) {
      configs[service][mode] = subData(data[index]);
    } else {
      configs[service] = {};
      configs[service][mode] = subData(data[index]);
    }
  }
  return configs;
}
function subData(data: Node[]) {
  let config: any = {};
  for (let i = 0; i < data.length; i++) {
    if (data[i].name == "Shu") {
      console.log(data[i].value)
    }
    if (data[i].children.length == 0) {
      if (data[i].name.split("#").length > 1) { // 数组
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
function setValue(src: any, dst: any) {
  for (let i in src) {
    if (dst[i] != undefined) {
      if (src[i] instanceof Object && dst[i] instanceof Object) {
        setValue(src[i], dst[i])
      } else if (typeof src[i] == typeof dst[i]){
        dst[i] = src[i]
      } else {
        console.log("mistake type: ", i)
      }
    }
  }
}

// utils
const handleSelect = (item: any) => {
  let s = item.index;
  showing.value = treeData.value[s];
};
const handleRes = (res: string) => {
  if (res) {
    ElNotification({
      title: "Error",
      message: res,
      type: "error",
    });
  } else {
    ElNotification({
      title: "Success",
      message: "保存成功",
      type: "success",
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
</style>
