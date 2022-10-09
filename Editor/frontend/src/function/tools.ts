/**
* @function 去除结构体空值字段
*/
export function trimDefault(data: any) {
    for (let i in data) {
      if (data[i] instanceof Object && !Array.isArray(data[i])) {
        trimDefault(data[i]);
        if (Object.keys(data[i]).length === 0) {
          delete data[i];
        }
      } else if (data[i] == 0 || data[i] == "" || data[i].length == 0) {
        delete data[i];
      }
    }
  }