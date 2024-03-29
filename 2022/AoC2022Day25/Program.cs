﻿var input = @"1=-=2=-1000=0
11=001122
120-
1-0
202-0==-2-
2-1010=2-2=01
112=-2
2-0=
1-=2-=-1-0--0=1=0000
1=0=0--021-0
11000101=210
21-111=2112=2==-
111010-2
2=10=-101-=--0
2-=-2000012=0022=
1-=11-01--2100
1=2211-2-===
2=--2-==-1
1=12---02=2
1==0=--11=01-1-
121-21021
12=1001=2=-=22
1=2-212=2-0211
20----11=
10=100=0=0
111
1=--011==2--212121=
2=-10=210200-2-=0
1=-122=001-1
2-0=0121=-0=0
2-=20--1-=21-22=1=
20011
12=1--=-0--10-0=1-=
2002=100=-00-
202
1=2==21-
1=221--01=00-10=
1--0-1=
2-==11=222---
21-10-=22
1=--22-0-
2-0-
1-0=-2
20-02102
1=0=20----21=12=-
20=111-12=21101=
111=1=-2=0=--21
11-
10-1=01=-
1222-10
1=2-1
20
101==12-1-0
1-=22=--=22=1=22
1200-010-10=021
1=2-10=-02--=-
12-0=000=0
1==10=00===1--
2-1=-=1===1=-0=---
1=11=120
2===002010=-=0
1==00212=01-002=221
10020221===-
1--01=-=0-20-101-=-
112=101
10-
1220=1
2=
1=1120=--0
11-21200=
2==011-10-0==0=0=
12-=102
100-022
1=1
201-0--=-
2-02
21=11=00020
12=20-
1101=00-1
1=0=-11--=-010-==1
1-2-0=00=-=220=11-
2-2
12-2
12=10=2-=
20222110-10002
1002=0
12=11-=012
1==
1=00-01
110101222-020--111
2-0
1-12=1
222-10-12=00
11=1=1
10=2-00-121-2-=2
1---22-1-0=00-1220
2-=2210=-==2=010=
12201-111-220
2=122=022=22=12-1
211===2=020
121-2-0-=0==2=1
1-0221200-21100-
10
1-11
2===2=-2-
1-0-2021-21==
1==-00-=2-21===1200
22
1222=20=120=1-
11=120001012==01-1
2=-1--00
10==121--221
1=2100102=20012
2==1-
1-
10-2--0";


var testInput = @"1=-0-2
12111
2=0=
21
2=01
111
20012
112
1=-1=
1-12
12
1=
122";


var parsed = input.Split('\n');

var numbers = parsed.Select(line =>
{
    var i = line.Length - 1L;
    var lineNb = 0L;
    foreach (var c in line)
    {
        var nb = GetSnafuNumber(c);
        nb *= (long)Math.Pow(5, i);

        lineNb += nb;
        i--;
    }

    return lineNb;

}).ToList();

var res = numbers.Sum();

Console.WriteLine($"Decimal {res}");

// Simple base conversion
var quotient = res;
var remainder = 0L;
var numberBase5Str = new char[1000];
for (int j = 0; j < 1000; j++)
{
    numberBase5Str[j] = ' ';
}
var i = 999;

while (quotient != 0)
{
    remainder = quotient % 5;
    quotient /= 5;
    numberBase5Str[i--] = remainder.ToString().First();
}

var resBase5 = new string(numberBase5Str).Trim();
Console.WriteLine($"Base5 {resBase5}");


// Complex snafu conversion

var numberBaseSnafuStr = new char[1000];
for (int j = 0; j < 1000; j++)
{
    numberBaseSnafuStr[j] = ' ';
}

for (int j = 10; j < resBase5.Length + 10; j++)
{
    var c = resBase5[j - 10];
    ComputeSnafuNumber(j, numberBaseSnafuStr, c);
}

var resSnafu = new string(numberBaseSnafuStr).Trim();
Console.WriteLine($"Base5 {resSnafu}");


void ComputeSnafuNumber(int pos, char[] snafuNb, char base5Nb)
{
    while (true)
    {
        if (base5Nb is '0' or '1' or '2')
        {
            var base5Input = int.Parse(base5Nb.ToString());

            var snafuCurrent =
                snafuNb[pos] == ' ' ? 0 :
                snafuNb[pos] == '-' ? -1 :
                snafuNb[pos] == '=' ? -2 : int.Parse(snafuNb[pos].ToString());

            var add = base5Input + snafuCurrent;

            if (add == 3)
            {
                snafuNb[pos] = '=';
                pos = pos - 1;
                base5Nb = '1';
                continue;
            }

            if (add == 4)
            {
                snafuNb[pos] = '-';
                pos = pos - 1;
                base5Nb = '1';
                continue;
            }


            snafuNb[pos] = add.ToString().First();
            return;
        }

        if (base5Nb == '3')
        {
            snafuNb[pos] = '=';
            pos = pos - 1;
            base5Nb = '1';
            continue;
        }

        if (base5Nb == '4')
        {
            snafuNb[pos] = '-';
            pos = pos - 1;
            base5Nb = '1';
            continue;
        }

        break;
    }
}

long GetSnafuNumber(char c)
{
    if (c == '0' || c == '1' || c == '2') return long.Parse(c.ToString());

    if (c == '-') return -1L;

    if (c == '=') return -2L;

    throw new Exception();
}