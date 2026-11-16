<script setup lang="ts">
import api from '@/api/api';
import type { CreateProductRequest, Product, UpdateProductRequest } from '@/models/product';
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';


const router = useRouter();

const products = ref<Product[]>([]);

const id = ref<number>(0);
const name = ref<string>('');
const description = ref<string>('');
const price = ref<number>(0);
const amount = ref<number>(0);
const isActive = ref<boolean>(true);
const createDate = ref<Date>(new Date());



async function getProducts(){
    try{
        const response = await api.get<Product[]>('/Products');
        products.value = response.data;
    }
    catch(error){
        console.error('Error fetching products:', error);
        alert('An error occurred while fetching products. Please try again.');
    }
}

async function saveProduct(){
    try{
        if(id.value === 0){
            const request : CreateProductRequest = {
                name: name.value,
                description: description.value,
                price: price.value,
                amount: amount.value,
                isActive: isActive.value,
                createDate: new Date()
            };
            const response = await api.post('/Products', request);
            getProducts();
        }
        else{
            const request : UpdateProductRequest = {
                id: id.value,
                name: name.value,
                description: description.value,
                price: price.value,
                amount: amount.value,
                isActive: isActive.value,
                createDate: createDate.value,
                updateDate: new Date()
            };
            const response = await api.put(`/Products/${id.value}`, request);
            getProducts();
        }
    }
    catch(error){
        console.error('Error saving product:', error);
        alert('An error occurred while saving the product. Please try again.');
    }
}

async function deleteProduct(id:number){
    try{
        const response = await api.delete(`/Products/${id}`);
        getProducts();
    }
    catch(error){
        console.error('Error deleting product:', error);
        alert('An error occurred while deleting the product. Please try again.');
    }
}

function editProduct(product: Product){
    id.value = product.id;
    name.value = product.name;
    description.value = product.description;
    price.value = product.price;
    amount.value = product.amount;
    isActive.value = product.isActive;
    createDate.value = new Date(product.createDate);
}

function clearForm(){
    id.value = 0;
    name.value = '';
    description.value = '';
    price.value = 0;
    amount.value = 0;
    isActive.value = true;
    createDate.value = new Date();
}

function logout(){
    localStorage.removeItem('token');
    router.push('/login');
}

onMounted(()=>{
    getProducts()
})

</script>

<template>
    <h1>Product Management</h1>
    <button @click="logout">Logout</button>
    <hr>
    <h2>Product Form</h2>
    <label for="name">Name:</label>
    <input type="text" v-model="name" id="name" placeholder="Name" required />
    <br><br>
    <label for="description">Description:</label>
    <input type="text" v-model="description" id="description" placeholder="Description" required />
    <br><br>
    <label for="price">Price:</label>
    <input type="number" v-model.number="price" id="price" placeholder="Price" required />
    <br><br>
    <label for="amount">Amount:</label>
    <input type="number" v-model.number="amount" id="amount" placeholder="Amount" required />
    <br><br>
    <label for="isActive">Active:</label>
    <input type="checkbox" v-model="isActive" id="isActive" />
    <br><br>
    <button @click="saveProduct">Save</button>
    <button @click="clearForm" style="margin-left: 5px;">Clear</button>
    <hr>
    <h2>Product List</h2>
    <table border="1">
        <thead>
            <tr>
                <th>ID</th>
                <th>Name</th>
                <th>Description</th>
                <th>Price</th>
                <th>Amount</th>
                <th>Active</th>
                <th>Create Date</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            <tr v-for="product in products" :key="product.id">
                <td>{{ product.id }}</td>
                <td>{{ product.name }}</td>
                <td>{{ product.description }}</td>
                <td>{{ product.price }}</td>
                <td>{{ product.amount }}</td>
                <td>{{ product.isActive ? 'Yes' : 'No' }}</td>
                <td>{{ new Date(product.createDate).toLocaleString() }}</td>
                <td>
                    <button @click="editProduct(product)">Edit</button>
                    <button @click="deleteProduct(product.id)" style="margin-left: 5px;">Delete</button>
                </td>
            </tr>
        </tbody>
    </table>
</template>

<style scoped></style>
