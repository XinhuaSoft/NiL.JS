async function a() {
    $.sleep(1000);
    return 'result of a';
}

async function b() {
    console.log(await a());
    return 'result of b';
}

b().then(console.log);