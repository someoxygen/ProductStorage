export interface Product{
    id: number;
    name: string;
    description: string;
    price: number;
    amount: number;
    isActive: boolean;
    createDate: Date;
    updateDate: Date;
}

export interface CreateProductRequest{
    name: string;
    description: string;
    price: number;
    amount: number;
    isActive: boolean;
    createDate: Date;
}

export interface UpdateProductRequest{
    id: number;
    name: string;
    description: string;
    price: number;
    amount: number;
    isActive: boolean;
    createDate: Date;
    updateDate: Date;
}