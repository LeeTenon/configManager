<template>
    <div class="layout-logo">
        <img class="logo-img" src="../assets/logo.png" />
        <div  class="website-name">
            配表工具
        </div>
    </div>
</template>

<script setup lang="ts">

</script>

<style scoped>
.layout-logo {
    width: 100%;
    height: 50px;
    display: flex;
    align-items: center;
    justify-content: center;
    box-sizing: border-box;
    padding: 10px;
    background:#fcfcfc;
}
.logo-img {
    width: 28px;
}
.website-name {
    padding-left: 4px;
    font-size: var(--el-font-size-extra-large);
    font-weight: 600;
}
</style>
