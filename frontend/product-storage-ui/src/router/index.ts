import LoginView from "@/views/LoginView.vue";
import ProductsView from "@/views/ProductsView.vue";
import RegisterView from "@/views/RegisterView.vue";
import { createRouter, createWebHistory } from "vue-router";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      redirect: "/products",
    },
    {
      path: "/register",
      component: RegisterView,
    },
    {
      path: "/login",
      component: LoginView,
    },
    {
      path: "/products",
      component: ProductsView,
      meta: { requiresAuth: true },
    },
  ],
});

router.beforeEach((to) => {
  if (to.meta.requiresAuth && !localStorage.getItem("token")) {
    return "/login";
  }
});

export default router;
