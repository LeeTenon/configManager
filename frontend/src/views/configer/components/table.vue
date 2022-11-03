<template>
    <el-table :data="data" style="width: 100%" :max-height="height" row-key="id" border size="small"
        :expand-row-keys="expendedRow" @expand-change="handleExpend">
        <el-table-column prop="name" label="配置项" style="width: 50%">
            <template #default="scope">
                <span v-if="scope.row.type == 'slice'" style="align-items: center">
                    <span>{{ scope.row.name }}</span>
                    <el-tag type="info" style="margin-left: 10px" size="small">Slice</el-tag>
                </span>
                <span v-else>{{ scope.row.name }}</span>
            </template>
        </el-table-column>
        <el-table-column prop="value" label="值" style="width: 50%">
            <template #default="scope">
                <div v-if="isNil(scope.row.children)" style="display: flex; align-items: center">
                    <el-select v-if="scope.row.type == 'enum'" class="common-value-input" placeholder="Select"
                        size="small" clearable v-model="scope.row.value"
                        :disabled="!isNil(scope.row.common) && !scope.row.force">
                        <el-option v-for="(value, key) in scope.row.spec" :key="value" :label="key" :value="key" />
                    </el-select>
                    <el-select v-else-if="scope.row.type == 'bool'" class="common-value-input" placeholder="Select"
                        size="small" clearable v-model="scope.row.value"
                        :disabled="!isNil(scope.row.common) && !scope.row.force">
                        <el-option v-for="value in boolEnum" :key="value.value" :label="value.lable"
                            :value="value.value" />
                    </el-select>
                    <el-input-number v-else-if="scope.row.type == 'number'" v-model="scope.row.value" size="small"
                        class="common-value-input" :disabled="isNil(scope.row.common) && !scope.row.force" />
                    <el-input v-else v-model="scope.row.value" size="small" class="common-value-input"
                        :disabled="!isNil(scope.row.common) || scope.row.force" />
                    <el-button v-if="!isUndefined(scope.row.common) && !scope.row.force" @click="scope.row.force = !scope.row.force"
                        size="small" style="margin-left: 15px;">
                        <span>修改</span>
                    </el-button>
                    <el-button v-else-if="!isUndefined(scope.row.common) && scope.row.force" @click="scope.row.force = !scope.row.force"
                        size="small" style="margin-left: 15px;">
                        <span>使用通用配置</span>
                    </el-button>
                </div>
            </template>
        </el-table-column>
    </el-table>
</template>

<script setup lang="ts">
import { defineProps, ref } from 'vue'
import { isUndefined } from "lodash";

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

const props = defineProps({
    data: {
        type: Array<TreeNode>,
    }
})

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

const expendedRow = ref([] as string[]);
const handleExpend = (row: any, isExpend: boolean) => {
    if (isExpend) {
        expendedRow.value.push(row.id);
    } else {
        remove(expendedRow.value, row.id);
    }
}
function remove(dst: string[], key: string) {
    var index = dst.indexOf(key);
    if (index > -1) {
        dst.splice(index, 1);
    }
}

function isNil(val: any) {
    return val == null || val == undefined
}

const height = ref();
height.value = document.body.clientHeight - 110 + "px";
window.onresize = () => {
    height.value = document.body.clientHeight - 110 + "px";
};
</script>

<style scoped>

</style>
