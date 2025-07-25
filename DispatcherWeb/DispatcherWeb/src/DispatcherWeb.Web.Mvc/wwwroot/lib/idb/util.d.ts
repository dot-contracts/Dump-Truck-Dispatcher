type Constructor = new (...args: any[]) => any;
type Func = (...args: any[]) => any;
declare const instanceOfAny: (object: any, constructors: Constructor[]) => boolean;
