const cartSpan = document.getElementById("cartIcon");


async function addToCart(id) {
    inputQuantityElement = document.getElementById("inputQuantity");
    const quantity = inputQuantityElement?.value;
    const data = await fetch(`/Customer/Cart/AddToCart?id=${id}&quantity=${quantity}`);
    const response = await data.json();

    if (response.success) {
        cartSpan.innerHTML = response.cartCount;
    }
    else {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: `Product Quantity out of Stock the available ${response.available}`,
        });
    }

}

const finalPriceElement = document.getElementById("finalPrice");

async function changeProductQuantity(id) {
    const productQuantitySelect = document.getElementById(`productQuantity-${id}`);
    const totalPriceElement = document.getElementById(`totalPrice-${id}`);
    const quantity = productQuantitySelect.value;

    const data = await fetch(`/Customer/Cart/ChangeProductQuantity?id=${id}&quantity=${quantity}`);

    const response = await data.json();

    if (response.success) {

        // Update the total price for this specific item
        totalPriceElement.innerHTML = `$${response.totalPrice.toFixed(2)}`;

        // Update the overall total price in the summary section
        const overallTotalPriceElement = document.getElementById("overallTotalPrice");
        overallTotalPriceElement.innerHTML = `$${response.overallTotalPrice.toFixed(2)}`;

        finalPriceElement.innerHTML = `$${response.overallTotalPrice.toFixed(2)}`;
    }
    else {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: `Product Quantity out of Stock the available ${response.available}`,
        });
    }
}



async function applyCoupone() {
    const couponCodeInput = document.getElementById("coupon-code");
    const discountAmountElement = document.getElementById("discount-amount");
    const couponCodeValue = couponCodeInput.value;
    const data = await fetch(`/Customer/Cart/ApplyCoupone?couponCode=${couponCodeValue}`);
    const response = await data.json();

    if (response.success) {
        discountAmountElement.innerHTML = `-${response.discount.toFixed(2)}`;
        finalPriceElement.innerHTML = `$${response.totalPriceAfterDiscount.toFixed(2)}`;
    }
    else {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: `Invalid coupon code`,
        });
    }
}


document.addEventListener("DOMContentLoaded", async () => {

    const data = await fetch("/Customer/Cart/GetCartCount");

    const response = await data.json();

    if (response.success) {
        cartSpan.innerHTML = response?.count;
    }

});


