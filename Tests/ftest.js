function* gen0()
{
    yield 1 + (yield 2);
}

for (var iter = gen0(), item; (item = iter.next()) && !item.done; )
    console.log(item.value);