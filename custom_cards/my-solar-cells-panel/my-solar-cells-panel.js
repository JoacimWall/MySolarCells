function t(t,e,a,s){var r,i=arguments.length,o=i<3?e:null===s?s=Object.getOwnPropertyDescriptor(e,a):s;if("object"==typeof Reflect&&"function"==typeof Reflect.decorate)o=Reflect.decorate(t,e,a,s);else for(var n=t.length-1;n>=0;n--)(r=t[n])&&(o=(i<3?r(o):i>3?r(e,a,o):r(e,a))||o);return i>3&&o&&Object.defineProperty(e,a,o),o}"function"==typeof SuppressedError&&SuppressedError;
/**
 * @license
 * Copyright 2019 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const e=globalThis,a=e.ShadowRoot&&(void 0===e.ShadyCSS||e.ShadyCSS.nativeShadow)&&"adoptedStyleSheets"in Document.prototype&&"replace"in CSSStyleSheet.prototype,s=Symbol(),r=new WeakMap;let i=class{constructor(t,e,a){if(this._$cssResult$=!0,a!==s)throw Error("CSSResult is not constructable. Use `unsafeCSS` or `css` instead.");this.cssText=t,this.t=e}get styleSheet(){let t=this.o;const e=this.t;if(a&&void 0===t){const a=void 0!==e&&1===e.length;a&&(t=r.get(e)),void 0===t&&((this.o=t=new CSSStyleSheet).replaceSync(this.cssText),a&&r.set(e,t))}return t}toString(){return this.cssText}};const o=(t,...e)=>{const a=1===t.length?t[0]:e.reduce((e,a,s)=>e+(t=>{if(!0===t._$cssResult$)return t.cssText;if("number"==typeof t)return t;throw Error("Value passed to 'css' function must be a 'css' function result: "+t+". Use 'unsafeCSS' to pass non-literal values, but take care to ensure page security.")})(a)+t[s+1],t[0]);return new i(a,t,s)},n=a?t=>t:t=>t instanceof CSSStyleSheet?(t=>{let e="";for(const a of t.cssRules)e+=a.cssText;return(t=>new i("string"==typeof t?t:t+"",void 0,s))(e)})(t):t,{is:l,defineProperty:d,getOwnPropertyDescriptor:h,getOwnPropertyNames:c,getOwnPropertySymbols:p,getPrototypeOf:u}=Object,m=globalThis,g=m.trustedTypes,v=g?g.emptyScript:"",_=m.reactiveElementPolyfillSupport,f=(t,e)=>t,y={toAttribute(t,e){switch(e){case Boolean:t=t?v:null;break;case Object:case Array:t=null==t?t:JSON.stringify(t)}return t},fromAttribute(t,e){let a=t;switch(e){case Boolean:a=null!==t;break;case Number:a=null===t?null:Number(t);break;case Object:case Array:try{a=JSON.parse(t)}catch(t){a=null}}return a}},b=(t,e)=>!l(t,e),$={attribute:!0,type:String,converter:y,reflect:!1,useDefault:!1,hasChanged:b};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */Symbol.metadata??=Symbol("metadata"),m.litPropertyMetadata??=new WeakMap;let k=class extends HTMLElement{static addInitializer(t){this._$Ei(),(this.l??=[]).push(t)}static get observedAttributes(){return this.finalize(),this._$Eh&&[...this._$Eh.keys()]}static createProperty(t,e=$){if(e.state&&(e.attribute=!1),this._$Ei(),this.prototype.hasOwnProperty(t)&&((e=Object.create(e)).wrapped=!0),this.elementProperties.set(t,e),!e.noAccessor){const a=Symbol(),s=this.getPropertyDescriptor(t,a,e);void 0!==s&&d(this.prototype,t,s)}}static getPropertyDescriptor(t,e,a){const{get:s,set:r}=h(this.prototype,t)??{get(){return this[e]},set(t){this[e]=t}};return{get:s,set(e){const i=s?.call(this);r?.call(this,e),this.requestUpdate(t,i,a)},configurable:!0,enumerable:!0}}static getPropertyOptions(t){return this.elementProperties.get(t)??$}static _$Ei(){if(this.hasOwnProperty(f("elementProperties")))return;const t=u(this);t.finalize(),void 0!==t.l&&(this.l=[...t.l]),this.elementProperties=new Map(t.elementProperties)}static finalize(){if(this.hasOwnProperty(f("finalized")))return;if(this.finalized=!0,this._$Ei(),this.hasOwnProperty(f("properties"))){const t=this.properties,e=[...c(t),...p(t)];for(const a of e)this.createProperty(a,t[a])}const t=this[Symbol.metadata];if(null!==t){const e=litPropertyMetadata.get(t);if(void 0!==e)for(const[t,a]of e)this.elementProperties.set(t,a)}this._$Eh=new Map;for(const[t,e]of this.elementProperties){const a=this._$Eu(t,e);void 0!==a&&this._$Eh.set(a,t)}this.elementStyles=this.finalizeStyles(this.styles)}static finalizeStyles(t){const e=[];if(Array.isArray(t)){const a=new Set(t.flat(1/0).reverse());for(const t of a)e.unshift(n(t))}else void 0!==t&&e.push(n(t));return e}static _$Eu(t,e){const a=e.attribute;return!1===a?void 0:"string"==typeof a?a:"string"==typeof t?t.toLowerCase():void 0}constructor(){super(),this._$Ep=void 0,this.isUpdatePending=!1,this.hasUpdated=!1,this._$Em=null,this._$Ev()}_$Ev(){this._$ES=new Promise(t=>this.enableUpdating=t),this._$AL=new Map,this._$E_(),this.requestUpdate(),this.constructor.l?.forEach(t=>t(this))}addController(t){(this._$EO??=new Set).add(t),void 0!==this.renderRoot&&this.isConnected&&t.hostConnected?.()}removeController(t){this._$EO?.delete(t)}_$E_(){const t=new Map,e=this.constructor.elementProperties;for(const a of e.keys())this.hasOwnProperty(a)&&(t.set(a,this[a]),delete this[a]);t.size>0&&(this._$Ep=t)}createRenderRoot(){const t=this.shadowRoot??this.attachShadow(this.constructor.shadowRootOptions);return((t,s)=>{if(a)t.adoptedStyleSheets=s.map(t=>t instanceof CSSStyleSheet?t:t.styleSheet);else for(const a of s){const s=document.createElement("style"),r=e.litNonce;void 0!==r&&s.setAttribute("nonce",r),s.textContent=a.cssText,t.appendChild(s)}})(t,this.constructor.elementStyles),t}connectedCallback(){this.renderRoot??=this.createRenderRoot(),this.enableUpdating(!0),this._$EO?.forEach(t=>t.hostConnected?.())}enableUpdating(t){}disconnectedCallback(){this._$EO?.forEach(t=>t.hostDisconnected?.())}attributeChangedCallback(t,e,a){this._$AK(t,a)}_$ET(t,e){const a=this.constructor.elementProperties.get(t),s=this.constructor._$Eu(t,a);if(void 0!==s&&!0===a.reflect){const r=(void 0!==a.converter?.toAttribute?a.converter:y).toAttribute(e,a.type);this._$Em=t,null==r?this.removeAttribute(s):this.setAttribute(s,r),this._$Em=null}}_$AK(t,e){const a=this.constructor,s=a._$Eh.get(t);if(void 0!==s&&this._$Em!==s){const t=a.getPropertyOptions(s),r="function"==typeof t.converter?{fromAttribute:t.converter}:void 0!==t.converter?.fromAttribute?t.converter:y;this._$Em=s;const i=r.fromAttribute(e,t.type);this[s]=i??this._$Ej?.get(s)??i,this._$Em=null}}requestUpdate(t,e,a,s=!1,r){if(void 0!==t){const i=this.constructor;if(!1===s&&(r=this[t]),a??=i.getPropertyOptions(t),!((a.hasChanged??b)(r,e)||a.useDefault&&a.reflect&&r===this._$Ej?.get(t)&&!this.hasAttribute(i._$Eu(t,a))))return;this.C(t,e,a)}!1===this.isUpdatePending&&(this._$ES=this._$EP())}C(t,e,{useDefault:a,reflect:s,wrapped:r},i){a&&!(this._$Ej??=new Map).has(t)&&(this._$Ej.set(t,i??e??this[t]),!0!==r||void 0!==i)||(this._$AL.has(t)||(this.hasUpdated||a||(e=void 0),this._$AL.set(t,e)),!0===s&&this._$Em!==t&&(this._$Eq??=new Set).add(t))}async _$EP(){this.isUpdatePending=!0;try{await this._$ES}catch(t){Promise.reject(t)}const t=this.scheduleUpdate();return null!=t&&await t,!this.isUpdatePending}scheduleUpdate(){return this.performUpdate()}performUpdate(){if(!this.isUpdatePending)return;if(!this.hasUpdated){if(this.renderRoot??=this.createRenderRoot(),this._$Ep){for(const[t,e]of this._$Ep)this[t]=e;this._$Ep=void 0}const t=this.constructor.elementProperties;if(t.size>0)for(const[e,a]of t){const{wrapped:t}=a,s=this[e];!0!==t||this._$AL.has(e)||void 0===s||this.C(e,void 0,a,s)}}let t=!1;const e=this._$AL;try{t=this.shouldUpdate(e),t?(this.willUpdate(e),this._$EO?.forEach(t=>t.hostUpdate?.()),this.update(e)):this._$EM()}catch(e){throw t=!1,this._$EM(),e}t&&this._$AE(e)}willUpdate(t){}_$AE(t){this._$EO?.forEach(t=>t.hostUpdated?.()),this.hasUpdated||(this.hasUpdated=!0,this.firstUpdated(t)),this.updated(t)}_$EM(){this._$AL=new Map,this.isUpdatePending=!1}get updateComplete(){return this.getUpdateComplete()}getUpdateComplete(){return this._$ES}shouldUpdate(t){return!0}update(t){this._$Eq&&=this._$Eq.forEach(t=>this._$ET(t,this[t])),this._$EM()}updated(t){}firstUpdated(t){}};k.elementStyles=[],k.shadowRootOptions={mode:"open"},k[f("elementProperties")]=new Map,k[f("finalized")]=new Map,_?.({ReactiveElement:k}),(m.reactiveElementVersions??=[]).push("2.1.2");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const w=globalThis,x=t=>t,S=w.trustedTypes,A=S?S.createPolicy("lit-html",{createHTML:t=>t}):void 0,P="$lit$",D=`lit$${Math.random().toFixed(9).slice(2)}$`,E="?"+D,T=`<${E}>`,I=document,R=()=>I.createComment(""),C=t=>null===t||"object"!=typeof t&&"function"!=typeof t,O=Array.isArray,L="[ \t\n\f\r]",U=/<(?:(!--|\/[^a-zA-Z])|(\/?[a-zA-Z][^>\s]*)|(\/?$))/g,K=/-->/g,F=/>/g,B=RegExp(`>|${L}(?:([^\\s"'>=/]+)(${L}*=${L}*(?:[^ \t\n\f\r"'\`<>=]|("|')|))|$)`,"g"),M=/'/g,Y=/"/g,W=/^(?:script|style|textarea|title)$/i,N=(t=>(e,...a)=>({_$litType$:t,strings:e,values:a}))(1),j=Symbol.for("lit-noChange"),H=Symbol.for("lit-nothing"),z=new WeakMap,V=I.createTreeWalker(I,129);function q(t,e){if(!O(t)||!t.hasOwnProperty("raw"))throw Error("invalid template strings array");return void 0!==A?A.createHTML(e):e}const J=(t,e)=>{const a=t.length-1,s=[];let r,i=2===e?"<svg>":3===e?"<math>":"",o=U;for(let e=0;e<a;e++){const a=t[e];let n,l,d=-1,h=0;for(;h<a.length&&(o.lastIndex=h,l=o.exec(a),null!==l);)h=o.lastIndex,o===U?"!--"===l[1]?o=K:void 0!==l[1]?o=F:void 0!==l[2]?(W.test(l[2])&&(r=RegExp("</"+l[2],"g")),o=B):void 0!==l[3]&&(o=B):o===B?">"===l[0]?(o=r??U,d=-1):void 0===l[1]?d=-2:(d=o.lastIndex-l[2].length,n=l[1],o=void 0===l[3]?B:'"'===l[3]?Y:M):o===Y||o===M?o=B:o===K||o===F?o=U:(o=B,r=void 0);const c=o===B&&t[e+1].startsWith("/>")?" ":"";i+=o===U?a+T:d>=0?(s.push(n),a.slice(0,d)+P+a.slice(d)+D+c):a+D+(-2===d?e:c)}return[q(t,i+(t[a]||"<?>")+(2===e?"</svg>":3===e?"</math>":"")),s]};class G{constructor({strings:t,_$litType$:e},a){let s;this.parts=[];let r=0,i=0;const o=t.length-1,n=this.parts,[l,d]=J(t,e);if(this.el=G.createElement(l,a),V.currentNode=this.el.content,2===e||3===e){const t=this.el.content.firstChild;t.replaceWith(...t.childNodes)}for(;null!==(s=V.nextNode())&&n.length<o;){if(1===s.nodeType){if(s.hasAttributes())for(const t of s.getAttributeNames())if(t.endsWith(P)){const e=d[i++],a=s.getAttribute(t).split(D),o=/([.?@])?(.*)/.exec(e);n.push({type:1,index:r,name:o[2],strings:a,ctor:"."===o[1]?et:"?"===o[1]?at:"@"===o[1]?st:tt}),s.removeAttribute(t)}else t.startsWith(D)&&(n.push({type:6,index:r}),s.removeAttribute(t));if(W.test(s.tagName)){const t=s.textContent.split(D),e=t.length-1;if(e>0){s.textContent=S?S.emptyScript:"";for(let a=0;a<e;a++)s.append(t[a],R()),V.nextNode(),n.push({type:2,index:++r});s.append(t[e],R())}}}else if(8===s.nodeType)if(s.data===E)n.push({type:2,index:r});else{let t=-1;for(;-1!==(t=s.data.indexOf(D,t+1));)n.push({type:7,index:r}),t+=D.length-1}r++}}static createElement(t,e){const a=I.createElement("template");return a.innerHTML=t,a}}function Z(t,e,a=t,s){if(e===j)return e;let r=void 0!==s?a._$Co?.[s]:a._$Cl;const i=C(e)?void 0:e._$litDirective$;return r?.constructor!==i&&(r?._$AO?.(!1),void 0===i?r=void 0:(r=new i(t),r._$AT(t,a,s)),void 0!==s?(a._$Co??=[])[s]=r:a._$Cl=r),void 0!==r&&(e=Z(t,r._$AS(t,e.values),r,s)),e}class Q{constructor(t,e){this._$AV=[],this._$AN=void 0,this._$AD=t,this._$AM=e}get parentNode(){return this._$AM.parentNode}get _$AU(){return this._$AM._$AU}u(t){const{el:{content:e},parts:a}=this._$AD,s=(t?.creationScope??I).importNode(e,!0);V.currentNode=s;let r=V.nextNode(),i=0,o=0,n=a[0];for(;void 0!==n;){if(i===n.index){let e;2===n.type?e=new X(r,r.nextSibling,this,t):1===n.type?e=new n.ctor(r,n.name,n.strings,this,t):6===n.type&&(e=new rt(r,this,t)),this._$AV.push(e),n=a[++o]}i!==n?.index&&(r=V.nextNode(),i++)}return V.currentNode=I,s}p(t){let e=0;for(const a of this._$AV)void 0!==a&&(void 0!==a.strings?(a._$AI(t,a,e),e+=a.strings.length-2):a._$AI(t[e])),e++}}class X{get _$AU(){return this._$AM?._$AU??this._$Cv}constructor(t,e,a,s){this.type=2,this._$AH=H,this._$AN=void 0,this._$AA=t,this._$AB=e,this._$AM=a,this.options=s,this._$Cv=s?.isConnected??!0}get parentNode(){let t=this._$AA.parentNode;const e=this._$AM;return void 0!==e&&11===t?.nodeType&&(t=e.parentNode),t}get startNode(){return this._$AA}get endNode(){return this._$AB}_$AI(t,e=this){t=Z(this,t,e),C(t)?t===H||null==t||""===t?(this._$AH!==H&&this._$AR(),this._$AH=H):t!==this._$AH&&t!==j&&this._(t):void 0!==t._$litType$?this.$(t):void 0!==t.nodeType?this.T(t):(t=>O(t)||"function"==typeof t?.[Symbol.iterator])(t)?this.k(t):this._(t)}O(t){return this._$AA.parentNode.insertBefore(t,this._$AB)}T(t){this._$AH!==t&&(this._$AR(),this._$AH=this.O(t))}_(t){this._$AH!==H&&C(this._$AH)?this._$AA.nextSibling.data=t:this.T(I.createTextNode(t)),this._$AH=t}$(t){const{values:e,_$litType$:a}=t,s="number"==typeof a?this._$AC(t):(void 0===a.el&&(a.el=G.createElement(q(a.h,a.h[0]),this.options)),a);if(this._$AH?._$AD===s)this._$AH.p(e);else{const t=new Q(s,this),a=t.u(this.options);t.p(e),this.T(a),this._$AH=t}}_$AC(t){let e=z.get(t.strings);return void 0===e&&z.set(t.strings,e=new G(t)),e}k(t){O(this._$AH)||(this._$AH=[],this._$AR());const e=this._$AH;let a,s=0;for(const r of t)s===e.length?e.push(a=new X(this.O(R()),this.O(R()),this,this.options)):a=e[s],a._$AI(r),s++;s<e.length&&(this._$AR(a&&a._$AB.nextSibling,s),e.length=s)}_$AR(t=this._$AA.nextSibling,e){for(this._$AP?.(!1,!0,e);t!==this._$AB;){const e=x(t).nextSibling;x(t).remove(),t=e}}setConnected(t){void 0===this._$AM&&(this._$Cv=t,this._$AP?.(t))}}class tt{get tagName(){return this.element.tagName}get _$AU(){return this._$AM._$AU}constructor(t,e,a,s,r){this.type=1,this._$AH=H,this._$AN=void 0,this.element=t,this.name=e,this._$AM=s,this.options=r,a.length>2||""!==a[0]||""!==a[1]?(this._$AH=Array(a.length-1).fill(new String),this.strings=a):this._$AH=H}_$AI(t,e=this,a,s){const r=this.strings;let i=!1;if(void 0===r)t=Z(this,t,e,0),i=!C(t)||t!==this._$AH&&t!==j,i&&(this._$AH=t);else{const s=t;let o,n;for(t=r[0],o=0;o<r.length-1;o++)n=Z(this,s[a+o],e,o),n===j&&(n=this._$AH[o]),i||=!C(n)||n!==this._$AH[o],n===H?t=H:t!==H&&(t+=(n??"")+r[o+1]),this._$AH[o]=n}i&&!s&&this.j(t)}j(t){t===H?this.element.removeAttribute(this.name):this.element.setAttribute(this.name,t??"")}}class et extends tt{constructor(){super(...arguments),this.type=3}j(t){this.element[this.name]=t===H?void 0:t}}class at extends tt{constructor(){super(...arguments),this.type=4}j(t){this.element.toggleAttribute(this.name,!!t&&t!==H)}}class st extends tt{constructor(t,e,a,s,r){super(t,e,a,s,r),this.type=5}_$AI(t,e=this){if((t=Z(this,t,e,0)??H)===j)return;const a=this._$AH,s=t===H&&a!==H||t.capture!==a.capture||t.once!==a.once||t.passive!==a.passive,r=t!==H&&(a===H||s);s&&this.element.removeEventListener(this.name,this,a),r&&this.element.addEventListener(this.name,this,t),this._$AH=t}handleEvent(t){"function"==typeof this._$AH?this._$AH.call(this.options?.host??this.element,t):this._$AH.handleEvent(t)}}class rt{constructor(t,e,a){this.element=t,this.type=6,this._$AN=void 0,this._$AM=e,this.options=a}get _$AU(){return this._$AM._$AU}_$AI(t){Z(this,t)}}const it=w.litHtmlPolyfillSupport;it?.(G,X),(w.litHtmlVersions??=[]).push("3.3.2");const ot=globalThis;
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */class nt extends k{constructor(){super(...arguments),this.renderOptions={host:this},this._$Do=void 0}createRenderRoot(){const t=super.createRenderRoot();return this.renderOptions.renderBefore??=t.firstChild,t}update(t){const e=this.render();this.hasUpdated||(this.renderOptions.isConnected=this.isConnected),super.update(t),this._$Do=((t,e,a)=>{const s=a?.renderBefore??e;let r=s._$litPart$;if(void 0===r){const t=a?.renderBefore??null;s._$litPart$=r=new X(e.insertBefore(R(),t),t,void 0,a??{})}return r._$AI(t),r})(e,this.renderRoot,this.renderOptions)}connectedCallback(){super.connectedCallback(),this._$Do?.setConnected(!0)}disconnectedCallback(){super.disconnectedCallback(),this._$Do?.setConnected(!1)}render(){return j}}nt._$litElement$=!0,nt.finalized=!0,ot.litElementHydrateSupport?.({LitElement:nt});const lt=ot.litElementPolyfillSupport;lt?.({LitElement:nt}),(ot.litElementVersions??=[]).push("4.2.2");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const dt=t=>(e,a)=>{void 0!==a?a.addInitializer(()=>{customElements.define(t,e)}):customElements.define(t,e)},ht={attribute:!0,type:String,converter:y,reflect:!1,hasChanged:b},ct=(t=ht,e,a)=>{const{kind:s,metadata:r}=a;let i=globalThis.litPropertyMetadata.get(r);if(void 0===i&&globalThis.litPropertyMetadata.set(r,i=new Map),"setter"===s&&((t=Object.create(t)).wrapped=!0),i.set(a.name,t),"accessor"===s){const{name:s}=a;return{set(a){const r=e.get.call(this);e.set.call(this,a),this.requestUpdate(s,r,t,!0,a)},init(e){return void 0!==e&&this.C(s,void 0,t,e),e}}}if("setter"===s){const{name:s}=a;return function(a){const r=this[s];e.call(this,a),this.requestUpdate(s,r,t,!0,a)}}throw Error("Unsupported decorator location: "+s)};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function pt(t){return(e,a)=>"object"==typeof a?ct(t,e,a):((t,e,a)=>{const s=e.hasOwnProperty(a);return e.constructor.createProperty(a,t),s?Object.getOwnPropertyDescriptor(e,a):void 0})(t,e,a)}
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function ut(t){return pt({...t,state:!0,attribute:!1})}const mt=o`
  :host {
    display: block;
    padding: 16px;
    --mdc-theme-primary: var(--primary-color);
  }

  .header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 16px;
  }

  .header h1 {
    margin: 0;
    font-size: 1.5em;
    font-weight: 400;
    color: var(--primary-text-color);
  }

  .tabs {
    display: flex;
    border-bottom: 1px solid var(--divider-color);
    margin-bottom: 16px;
  }

  .tab {
    padding: 12px 20px;
    cursor: pointer;
    border-bottom: 2px solid transparent;
    color: var(--secondary-text-color);
    font-size: 0.95em;
    font-weight: 500;
    transition: color 0.2s, border-color 0.2s;
    background: none;
    border-top: none;
    border-left: none;
    border-right: none;
  }

  .tab:hover {
    color: var(--primary-text-color);
  }

  .tab.active {
    color: var(--primary-color);
    border-bottom-color: var(--primary-color);
  }
`,gt=o`
  .card {
    background: var(--ha-card-background, var(--card-background-color, white));
    border-radius: var(--ha-card-border-radius, 12px);
    box-shadow: var(--ha-card-box-shadow, 0 2px 6px rgba(0,0,0,0.1));
    padding: 16px;
    margin-bottom: 16px;
  }

  .card h3 {
    margin: 0 0 12px 0;
    font-size: 1.1em;
    font-weight: 500;
    color: var(--primary-text-color);
  }

  .stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
    gap: 12px;
  }

  .stat-item {
    text-align: center;
    padding: 12px 8px;
    border-radius: 8px;
    background: var(--primary-background-color);
  }

  .stat-label {
    font-size: 0.8em;
    color: var(--secondary-text-color);
    margin-bottom: 4px;
  }

  .stat-value {
    font-size: 1.3em;
    font-weight: 600;
    color: var(--primary-text-color);
  }
`,vt=o`
  .table-controls {
    display: flex;
    gap: 12px;
    align-items: flex-end;
    flex-wrap: wrap;
    margin-bottom: 16px;
  }

  .input-group {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  .input-group label {
    font-size: 0.8em;
    color: var(--secondary-text-color);
  }

  .input-group input {
    padding: 8px 12px;
    border: 1px solid var(--divider-color);
    border-radius: 6px;
    background: var(--card-background-color, white);
    color: var(--primary-text-color);
    font-size: 0.9em;
  }

  button.btn {
    padding: 8px 16px;
    border: none;
    border-radius: 6px;
    background: var(--primary-color);
    color: var(--text-primary-color, white);
    cursor: pointer;
    font-size: 0.9em;
    font-weight: 500;
  }

  button.btn:hover {
    opacity: 0.9;
  }

  button.btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.85em;
  }

  th {
    text-align: left;
    padding: 8px 6px;
    border-bottom: 2px solid var(--divider-color);
    color: var(--secondary-text-color);
    font-weight: 500;
    white-space: nowrap;
  }

  td {
    padding: 6px;
    border-bottom: 1px solid var(--divider-color);
  }

  .table-wrapper {
    overflow-x: auto;
  }

  .pagination {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 16px;
    margin-top: 12px;
    font-size: 0.9em;
    color: var(--secondary-text-color);
  }

  .summary-row {
    background: var(--primary-background-color);
    font-weight: 600;
  }

  .loading {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }

  .no-data {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }
`,_t=o`
  .period-nav {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 16px;
    flex-wrap: wrap;
  }

  .period-tabs {
    display: flex;
    gap: 2px;
    background: var(--divider-color);
    border-radius: 8px;
    padding: 2px;
  }

  .period-tab {
    padding: 6px 14px;
    border: none;
    background: transparent;
    color: var(--secondary-text-color);
    cursor: pointer;
    border-radius: 6px;
    font-size: 0.85em;
    font-weight: 500;
    transition: background 0.15s, color 0.15s;
  }

  .period-tab:hover {
    color: var(--primary-text-color);
  }

  .period-tab.active {
    background: var(--primary-color);
    color: var(--text-primary-color, white);
  }

  .nav-arrow {
    padding: 4px 10px;
    border: 1px solid var(--divider-color);
    background: var(--ha-card-background, var(--card-background-color, white));
    color: var(--primary-text-color);
    cursor: pointer;
    border-radius: 6px;
    font-size: 1em;
    line-height: 1;
  }

  .nav-arrow:hover {
    background: var(--primary-background-color);
  }

  .period-label {
    font-weight: 600;
    font-size: 0.9em;
    color: var(--primary-text-color);
    min-width: 120px;
    text-align: center;
  }

  .fakta-columns {
    display: grid;
    grid-template-columns: 1fr 1fr 1fr;
    gap: 16px;
    align-items: start;
  }

  @media (max-width: 900px) {
    .fakta-columns {
      grid-template-columns: 1fr;
    }
  }

  .fakta-card {
    background: var(--ha-card-background, var(--card-background-color, white));
    border-radius: var(--ha-card-border-radius, 12px);
    box-shadow: var(--ha-card-box-shadow, 0 2px 6px rgba(0,0,0,0.1));
    padding: 16px;
  }

  .fakta-card h3 {
    margin: 0 0 12px 0;
    font-size: 1em;
    font-weight: 600;
    color: var(--primary-text-color);
  }

  .fakta-row {
    display: flex;
    justify-content: space-between;
    padding: 4px 0;
    font-size: 0.85em;
  }

  .fakta-row .label {
    color: var(--secondary-text-color);
  }

  .fakta-row .value {
    font-weight: 500;
    color: var(--primary-text-color);
    font-variant-numeric: tabular-nums;
  }

  .fakta-separator {
    border: none;
    border-top: 1px solid var(--divider-color);
    margin: 6px 0;
  }

  .fakta-summary {
    font-weight: 600;
    font-size: 0.9em;
  }

  .fakta-summary .value {
    font-weight: 700;
  }

  .sim-section {
    margin-bottom: 16px;
  }

  .sim-toggle-group {
    display: flex;
    gap: 2px;
    background: var(--divider-color);
    border-radius: 8px;
    padding: 2px;
    margin-bottom: 10px;
  }

  .sim-toggle {
    flex: 1;
    padding: 6px 10px;
    border: none;
    background: transparent;
    color: var(--secondary-text-color);
    cursor: pointer;
    border-radius: 6px;
    font-size: 0.8em;
    font-weight: 500;
    transition: background 0.15s, color 0.15s;
  }

  .sim-toggle.active {
    background: var(--primary-color);
    color: var(--text-primary-color, white);
  }

  .sim-slider-row {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 10px;
  }

  .sim-slider-row input[type="range"] {
    flex: 1;
  }

  .sim-slider-row .slider-value {
    min-width: 50px;
    text-align: right;
    font-size: 0.85em;
    font-weight: 500;
  }

  .sim-checkbox {
    display: flex;
    align-items: center;
    gap: 6px;
    margin-bottom: 10px;
    font-size: 0.85em;
    color: var(--secondary-text-color);
  }

  .sim-checkbox input {
    margin: 0;
  }

  button.sim-btn {
    width: 100%;
    padding: 8px 16px;
    border: none;
    border-radius: 6px;
    background: var(--primary-color);
    color: var(--text-primary-color, white);
    cursor: pointer;
    font-size: 0.85em;
    font-weight: 500;
    margin-bottom: 16px;
  }

  button.sim-btn:hover {
    opacity: 0.9;
  }

  button.sim-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .loading {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }

  .no-data {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }
`,ft=["January","February","March","April","May","June","July","August","September","October","November","December"],yt=["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"],bt=["JANUARI","FEBRUARI","MARS","APRIL","MAJ","JUNI","JULI","AUGUSTI","SEPTEMBER","OKTOBER","NOVEMBER","DECEMBER"],$t=["JAN.","FEB.","MAR.","APR.","MAJ","JUN.","JUL.","AUG.","SEP.","OKT.","NOV.","DEC."],kt={en:{"panel.title":"Solar Data","tab.overview":"Overview","tab.roi":"ROI","tab.fakta":"Facts","tab.sensors":"Sensors","tab.yearlyParams":"Yearly Params","tab.hourlyEnergy":"Hourly Energy","common.year":"Year","common.yes":"Yes","common.no":"No","common.loading":"Loading...","common.error":"Error","common.noData":"No data available","common.save":"Save","common.cancel":"Cancel","common.delete":"Delete","common.refresh":"Refresh","common.add":"Add","common.na":"N/A","common.never":"Never","chart.ownUse":"Own Use","chart.sold":"Sold","chart.today":"Today","chart.thisWeek":"This Week","chart.thisMonth":"This Month","chart.thisYear":"This Year","overview.loadingOverview":"Loading overview...","overview.noData":"No data available","overview.dbSummary":"Database Summary","overview.lastTibberSync":"Last Tibber Sync","overview.hourlyRecords":"Hourly Records","overview.firstRecord":"First Record","overview.lastRecord":"Last Record","overview.energySummary":"Energy Summary","overview.yearlyParams":"Yearly Financial Parameters","overview.taxReduction":"Tax Reduction","overview.gridComp":"Grid Comp.","overview.transferFee":"Transfer Fee","overview.energyTax":"Energy Tax","overview.installedKw":"Installed kW","hourly.title":"Hourly Energy Records","hourly.startDate":"Start Date","hourly.endDate":"End Date","hourly.loadBtn":"Load","hourly.loadingBtn":"Loading...","hourly.selectDateRange":"Select a date range and click Load","hourly.timestamp":"Timestamp","hourly.purchasedKwh":"Purchased kWh","hourly.costSek":"Cost SEK","hourly.soldKwh":"Sold kWh","hourly.profitSek":"Profit SEK","hourly.ownUseKwh":"Own Use kWh","hourly.savedSek":"Saved SEK","hourly.priceLevel":"Price Level","hourly.synced":"Synced","hourly.enriched":"Enriched","hourly.prev":"Prev","hourly.next":"Next","hourly.pageInfo":"Page {0} of {1} ({2} records)","sensors.loadingSensors":"Loading sensor configuration...","sensors.title":"Sensor Configuration","sensors.infoBox":"Only the <strong>production</strong> sensor is required — it is used to calculate <strong>production_own_use</strong> (total production minus grid export). All other data (grid export, grid import) comes from the <strong>Tibber API</strong> by default. You can optionally override grid export/import with HA sensors for higher accuracy, and add battery sensors if you have a battery. Sensors are configured in the integration setup flow.","sensors.status":"Status","sensors.role":"Role","sensors.description":"Description","sensors.entityId":"Entity ID","sensors.currentState":"Current State","sensors.lastStoredReading":"Last Stored Reading","sensors.required":"Required","sensors.optional":"Optional","sensors.requiredMissing":"Required Sensor Missing","sensors.requiredMissingText":"The <strong>production</strong> sensor is not configured. Without it, <strong>production_own_use</strong> cannot be calculated. Please configure it in the integration setup flow.","sensors.usingTibberApi":"Using Tibber API","sensors.notConfigured":"Not configured","params.loadingParams":"Loading yearly parameters...","params.waitingForData":"Waiting for data...","params.title":"Yearly Financial Parameters","params.addYear":"Add Year","params.selectYear":"Select year...","params.noParams":"No yearly parameters configured yet. Add a year above.","params.taxReduction":"Tax Reduction","params.gridComp":"Grid Comp.","params.transferFee":"Transfer Fee","params.energyTax":"Energy Tax","params.installedKw":"Installed kW","params.editYear":"Edit {0}","params.taxReductionLabel":"Tax Reduction (SEK/kWh)","params.gridCompLabel":"Grid Compensation (SEK/kWh)","params.transferFeeLabel":"Transfer Fee (SEK/kWh)","params.energyTaxLabel":"Energy Tax (SEK/kWh)","params.installedKwLabel":"Installed kW","params.saving":"Saving...","params.deleteConfirm":"Delete parameters for {0}?","roi.loadingRoi":"Loading ROI projection...","roi.noData":"No ROI projection data available.","roi.title":"ROI Projection","roi.investment":"Investment: <strong>{0} SEK</strong>","roi.howCalculated":"How is the ROI calculated?","roi.explainHistorical":"Historical years use actual production and price data from Tibber. If the current year is incomplete, each missing month is filled using the average price and production for that specific month from up to 3 prior years. If no prior data exists for a month, the previous month’s price is used as fallback.","roi.explainSoldPrice":"Sold price = spot price + grid compensation (nätnytta) + tax reduction (skattereduktion, until 2026).","roi.explainOwnUsePrice":"Own use price = avoided purchase cost (spot price + transfer fee + energy tax). If a battery is present, it is the average of own-use and battery savings per kWh.","roi.explainFuture":"Future years are projected using monthly average prices and production from up to 3 of the most recent historical years. Each calendar month is averaged across however many years have data for it. Price development and panel degradation are then applied per month and summed to yearly totals. This captures seasonal variation — summer has high production but typically lower prices, winter the opposite.","roi.explainPriceDev":"Prices increase each year by the price development percentage. E.g. 5% means next year’s price = this year’s price × 1.05.","roi.explainPanelDeg":"Production decreases each year by the panel degradation percentage. E.g. 0.25% means next year’s production = this year’s × 0.9975.","roi.explainSavingsSold":"Savings sold = production sold × average sold price","roi.explainSavingsOwnUse":"Savings own use = own use production × average own use price","roi.explainRemaining":"Remaining = investment − cumulative total savings","roi.explainRoiYear":"The ROI year (green row) is when cumulative savings exceed the investment.","roi.explainTaxReduction":"Tax reduction (skattereduktion) is removed from sold price starting 2026.","roi.explainNote":"Note: The table shows yearly weighted averages, but all savings calculations use monthly granularity for accuracy.","roi.priceDevLabel":"Price development (%/year)","roi.panelDegLabel":"Panel degradation (%/year)","roi.calculating":"Calculating...","roi.calculate":"Calculate","roi.colIndex":"#","roi.colYear":"Year","roi.colAvgPriceSold":"Avg price sold","roi.colAvgPriceOwnUse":"Avg price own use","roi.colProdSold":"Prod. sold (kWh)","roi.colProdOwnUse":"Prod. own use (kWh)","roi.colSavingsSold":"Savings sold (SEK)","roi.colSavingsOwnUse":"Savings own use (SEK)","roi.colReturnPct":"Return %","roi.colRemaining":"Remaining (SEK)","fakta.periodToday":"Today","fakta.periodDay":"Day","fakta.periodWeek":"Week","fakta.periodMonth":"Month","fakta.periodYear":"Year","fakta.todayLabel":"TODAY","fakta.productionTitle":"Production and consumption","fakta.sold":"Sold","fakta.ownUse":"Own use","fakta.batteryCharge":"Battery charge","fakta.batteryUse":"Battery use","fakta.purchased":"Purchased","fakta.production":"Production","fakta.consumption":"Consumption","fakta.balance":"Balance (prod. − purchased)","fakta.costTitle":"Costs and revenue","fakta.prodSold":"Prod. sold","fakta.prodGridComp":"Prod. grid comp.","fakta.prodTaxReduction":"Prod. tax reduction","fakta.ownUseSpot":"Own use spot price","fakta.ownUseTransfer":"Own use transfer","fakta.ownUseEnergyTax":"Own use energy tax","fakta.battSpot":"Batt. use spot price","fakta.battTransfer":"Batt. use transfer","fakta.battEnergyTax":"Batt. use energy tax","fakta.purchasedCost":"Purchased elec. cost","fakta.purchasedTransfer":"Purchased transfer fee","fakta.purchasedTax":"Purchased energy tax","fakta.costProduction":"Production","fakta.costConsumption":"Consumption","fakta.costBalance":"Balance (prod. − purchased)","fakta.simTitle":"Simulation","fakta.simTitleWithBattery":"Simulation (with {0} kWh battery)","fakta.simTitleWithoutBattery":"Simulation (without battery)","fakta.simAddBattery":"With battery","fakta.simRemoveBattery":"Without battery","fakta.simCalculating":"Calculating...","fakta.simCalculate":"Calculate","fakta.factsTitle":"Facts","fakta.factsProductionIndex":"Production index (prod/day)","fakta.factsAvgPriceSold":"Avg price sold","fakta.factsAvgPricePurchased":"Avg price purchased","fakta.factsAvgPriceOwnUse":"Avg price own use","fakta.factsPeakReduction":"Own use peak consumption reduction"},sv:{"panel.title":"Soldata","tab.overview":"Översikt","tab.roi":"ROI","tab.fakta":"Fakta","tab.sensors":"Sensorer","tab.yearlyParams":"Årsparametrar","tab.hourlyEnergy":"Timdata","common.year":"År","common.yes":"Ja","common.no":"Nej","common.loading":"Laddar...","common.error":"Fel","common.noData":"Ingen data tillgänglig","common.save":"Spara","common.cancel":"Avbryt","common.delete":"Ta bort","common.refresh":"Uppdatera","common.add":"Lägg till","common.na":"N/A","common.never":"Aldrig","chart.ownUse":"Eget anv.","chart.sold":"Såld","chart.today":"Idag","chart.thisWeek":"Denna vecka","chart.thisMonth":"Denna månad","chart.thisYear":"I år","overview.loadingOverview":"Laddar översikt...","overview.noData":"Ingen data tillgänglig","overview.dbSummary":"Databasöversikt","overview.lastTibberSync":"Senaste Tibber-synk","overview.hourlyRecords":"Timposter","overview.firstRecord":"Första post","overview.lastRecord":"Senaste post","overview.energySummary":"Energiöversikt","overview.yearlyParams":"Årliga ekonomiska parametrar","overview.taxReduction":"Skattereduktion","overview.gridComp":"Nätnytta","overview.transferFee":"Överföring","overview.energyTax":"Energiskatt","overview.installedKw":"Installerad kW","hourly.title":"Timdata för energi","hourly.startDate":"Startdatum","hourly.endDate":"Slutdatum","hourly.loadBtn":"Ladda","hourly.loadingBtn":"Laddar...","hourly.selectDateRange":"Välj datumintervall och klicka Ladda","hourly.timestamp":"Tidstämpel","hourly.purchasedKwh":"Köpt kWh","hourly.costSek":"Kostnad SEK","hourly.soldKwh":"Såld kWh","hourly.profitSek":"Intäkt SEK","hourly.ownUseKwh":"Eget anv. kWh","hourly.savedSek":"Besparing SEK","hourly.priceLevel":"Prisgrupp","hourly.synced":"Synkad","hourly.enriched":"Berikad","hourly.prev":"Föreg.","hourly.next":"Nästa","hourly.pageInfo":"Sida {0} av {1} ({2} poster)","sensors.loadingSensors":"Laddar sensorkonfiguration...","sensors.title":"Sensorkonfiguration","sensors.infoBox":"Bara <strong>produktionssensorn</strong> krävs — den används för att beräkna <strong>production_own_use</strong> (total produktion minus nätexport). All annan data (nätexport, nätimport) hämtas från <strong>Tibber API</strong> som standard. Du kan valfritt åsidosätta nätexport/import med HA-sensorer för högre noggrannhet, och lägga till batterisensorer om du har ett batteri. Sensorer konfigureras i integrationsflödet.","sensors.status":"Status","sensors.role":"Roll","sensors.description":"Beskrivning","sensors.entityId":"Entitets-ID","sensors.currentState":"Aktuellt värde","sensors.lastStoredReading":"Senast lagrad avläsning","sensors.required":"Obligatorisk","sensors.optional":"Valfri","sensors.requiredMissing":"Obligatorisk sensor saknas","sensors.requiredMissingText":"<strong>Produktionssensorn</strong> är inte konfigurerad. Utan den kan inte <strong>production_own_use</strong> beräknas. Konfigurera den i integrationsflödet.","sensors.usingTibberApi":"Använder Tibber API","sensors.notConfigured":"Ej konfigurerad","params.loadingParams":"Laddar årsparametrar...","params.waitingForData":"Väntar på data...","params.title":"Årliga ekonomiska parametrar","params.addYear":"Lägg till år","params.selectYear":"Välj år...","params.noParams":"Inga årsparametrar konfigurerade än. Lägg till ett år ovan.","params.taxReduction":"Skattereduktion","params.gridComp":"Nätnytta","params.transferFee":"Överföring","params.energyTax":"Energiskatt","params.installedKw":"Installerad kW","params.editYear":"Redigera {0}","params.taxReductionLabel":"Skattereduktion (SEK/kWh)","params.gridCompLabel":"Nätnytta (SEK/kWh)","params.transferFeeLabel":"Överföringsavgift (SEK/kWh)","params.energyTaxLabel":"Energiskatt (SEK/kWh)","params.installedKwLabel":"Installerad kW","params.saving":"Sparar...","params.deleteConfirm":"Ta bort parametrar för {0}?","roi.loadingRoi":"Laddar ROI-prognos...","roi.noData":"Ingen ROI-prognosdata tillgänglig.","roi.title":"ROI-prognos","roi.investment":"Investering: <strong>{0} SEK</strong>","roi.howCalculated":"Hur beräknas ROI?","roi.explainHistorical":"Historiska år använder faktisk produktion och prisdata från Tibber. Om innevärande år är ofullständigt fylls varje saknad månad med snittvärden för pris och produktion för den specifika månaden från upp till 3 föregående år. Om ingen data finns för en månad används föregående månads pris som reserv.","roi.explainSoldPrice":"Säljpris = spotpris + nätnytta + skattereduktion (till 2026).","roi.explainOwnUsePrice":"Eget anv. pris = undvikt inköpskostnad (spotpris + överföringsavgift + energiskatt). Om batteri finns är det genomsnittet av eget anv. och batteribesparing per kWh.","roi.explainFuture":"Framtida år projiceras med månadssnitt för priser och produktion från upp till 3 av de senaste historiska åren. Varje kalendermånad medelviktas över de år som har data. Prisutveckling och paneldegradation appliceras per månad och summeras till årstotaler. Detta fångar säsongsvariationer — sommaren har hög produktion men lägre priser, vintern tvärtom.","roi.explainPriceDev":"Priserna ökar varje år med prisutvecklingsprocenten. T.ex. 5% innebär att nästa års pris = detta års pris × 1,05.","roi.explainPanelDeg":"Produktionen minskar varje år med paneldegraderingsprocenten. T.ex. 0,25% innebär att nästa års produktion = detta års × 0,9975.","roi.explainSavingsSold":"Besparing såld = produktion såld × genomsnittligt säljpris","roi.explainSavingsOwnUse":"Besparing eget anv. = eget användning × genomsnittligt eget anv. pris","roi.explainRemaining":"Återstående = investering − ackumulerad total besparing","roi.explainRoiYear":"ROI-året (grön rad) är när ackumulerad besparing överstiger investeringen.","roi.explainTaxReduction":"Skattereduktionen tas bort från säljpriset från 2026.","roi.explainNote":"Obs: Tabellen visar årliga viktade snittvärden, men alla besparingsberäkningar använder månadsgranularitet för noggrannhet.","roi.priceDevLabel":"Prisutveckling (%/år)","roi.panelDegLabel":"Paneldegradation (%/år)","roi.calculating":"Beräknar...","roi.calculate":"Beräkna","roi.colIndex":"#","roi.colYear":"År","roi.colAvgPriceSold":"Snittpris såld","roi.colAvgPriceOwnUse":"Snittpris eget anv.","roi.colProdSold":"Prod. såld (kWh)","roi.colProdOwnUse":"Prod. eget anv. (kWh)","roi.colSavingsSold":"Besparing såld (SEK)","roi.colSavingsOwnUse":"Besparing eget anv. (SEK)","roi.colReturnPct":"Avkastning %","roi.colRemaining":"Återstående (SEK)","fakta.periodToday":"Idag","fakta.periodDay":"Dag","fakta.periodWeek":"Vecka","fakta.periodMonth":"Månad","fakta.periodYear":"År","fakta.todayLabel":"IDAG","fakta.productionTitle":"Produktion och konsumtion","fakta.sold":"Såld","fakta.ownUse":"Eget anv.","fakta.batteryCharge":"Batteriladdning","fakta.batteryUse":"Batteri anv.","fakta.purchased":"Köpt","fakta.production":"Produktion","fakta.consumption":"Konsumtion","fakta.balance":"Balans (prod. − inköp)","fakta.costTitle":"Kostnader och intäkter","fakta.prodSold":"Prod sålt","fakta.prodGridComp":"Prod nätnytta","fakta.prodTaxReduction":"Prod energiskatt","fakta.ownUseSpot":"Eget anv. spotpris","fakta.ownUseTransfer":"Eget anv. överföring","fakta.ownUseEnergyTax":"Eget anv. energiskatt","fakta.battSpot":"Batt. anv. spotpris","fakta.battTransfer":"Batt. anv. överföring","fakta.battEnergyTax":"Batt. anv. energiskatt","fakta.purchasedCost":"Köpt el kostnad","fakta.purchasedTransfer":"Köpt överföringavgift","fakta.purchasedTax":"Köpt energiskatt","fakta.costProduction":"Produktion","fakta.costConsumption":"Konsumtion","fakta.costBalance":"Balans (prod. − inköpt)","fakta.simTitle":"Simulering","fakta.simTitleWithBattery":"Simulering (med {0} kWh batteri i beräkning)","fakta.simTitleWithoutBattery":"Simulering (utan batteri i beräkning)","fakta.simAddBattery":"Med batteri","fakta.simRemoveBattery":"Utan batteri","fakta.simCalculating":"Beräknar...","fakta.simCalculate":"Beräkna","fakta.factsTitle":"Fakta","fakta.factsProductionIndex":"Produktionsindex (prod/dag)","fakta.factsAvgPriceSold":"Snittpris såld","fakta.factsAvgPricePurchased":"Snittpris köpt","fakta.factsAvgPriceOwnUse":"Snittpris eget användning","fakta.factsPeakReduction":"Eget anv. reducering avg. förb."}};function wt(t){return"sv"===(t?.language||t?.locale?.language||"en")?"sv":"en"}function xt(t){return"sv"===wt(t)?"sv-SE":"en-US"}function St(t,e,...a){const s=wt(t);let r=kt[s][e]||kt.en[e]||e;for(let t=0;t<a.length;t++)r=r.replace(`{${t}}`,String(a[t]));return r}function At(t,e){return"sv"===wt(t)?$t[e]:yt[e]}const Pt=[{key:"own_use_kwh",labelKey:"chart.ownUse",color:"#4285f4",unit:"kWh"},{key:"sold_kwh",labelKey:"chart.sold",color:"#34a853",unit:"kWh"},{key:"own_use_sek",labelKey:"chart.ownUse",color:"#8ab4f8",unit:"SEK"},{key:"sold_sek",labelKey:"chart.sold",color:"#f4a742",unit:"SEK"}],Dt=[{key:"today",labelKey:"chart.today"},{key:"this_week",labelKey:"chart.thisWeek"},{key:"this_month",labelKey:"chart.thisMonth"},{key:"this_year",labelKey:"chart.thisYear"}];let Et=class extends nt{render(){if(!this.data)return H;const t=this._getMax("kwh"),e=this._getMax("sek");return N`
      <div class="chart-container">
        ${Dt.map(a=>this._renderPeriod(a,this.data[a.key],t,e))}
      </div>
      <div class="legend">
        ${Pt.map(t=>N`
            <div class="legend-item">
              <div class="legend-swatch" style="background:${t.color}"></div>
              ${St(this.hass,t.labelKey)} (${t.unit})
            </div>
          `)}
      </div>
    `}_renderPeriod(t,e,a,s){return N`
      <div class="period-column">
        <div class="period-label">${St(this.hass,t.labelKey)}</div>
        <div class="bars">
          ${Pt.map(t=>{const r=e[t.key],i="kWh"===t.unit?a:s;return N`
              <div class="bar-wrapper">
                <div
                  class="bar"
                  style="height:${i>0?r/i*100:0}%;background:${t.color}"
                ></div>
                <div class="bar-value">${this._formatValue(r,t.unit)}</div>
              </div>
            `})}
        </div>
      </div>
    `}_getMax(t){if(!this.data)return 0;const e="kwh"===t?["own_use_kwh","sold_kwh"]:["own_use_sek","sold_sek"];let a=0;for(const t of Dt){const s=this.data[t.key];for(const t of e)s[t]>a&&(a=s[t])}return a}_formatValue(t,e){return t>=1e3?`${(t/1e3).toFixed(1)}k`:t>=10?t.toFixed(0):t.toFixed(1)}};Et.styles=o`
    :host {
      display: block;
    }

    .chart-container {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 16px;
    }

    @media (max-width: 600px) {
      .chart-container {
        grid-template-columns: repeat(2, 1fr);
      }
    }

    .period-column {
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .period-label {
      font-size: 0.85em;
      font-weight: 500;
      color: var(--primary-text-color);
      margin-bottom: 8px;
    }

    .bars {
      display: flex;
      gap: 4px;
      align-items: flex-end;
      height: 120px;
      width: 100%;
      justify-content: center;
    }

    .bar-wrapper {
      display: flex;
      flex-direction: column;
      align-items: center;
      flex: 1;
      max-width: 36px;
      height: 100%;
      justify-content: flex-end;
    }

    .bar {
      width: 100%;
      border-radius: 3px 3px 0 0;
      min-height: 2px;
      transition: height 0.3s ease;
    }

    .bar-value {
      font-size: 0.65em;
      color: var(--secondary-text-color);
      margin-top: 4px;
      white-space: nowrap;
      text-align: center;
    }

    .legend {
      display: flex;
      justify-content: center;
      gap: 16px;
      margin-top: 16px;
      flex-wrap: wrap;
    }

    .legend-item {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 0.75em;
      color: var(--secondary-text-color);
    }

    .legend-swatch {
      width: 10px;
      height: 10px;
      border-radius: 2px;
    }
  `,t([pt({attribute:!1})],Et.prototype,"hass",void 0),t([pt({attribute:!1})],Et.prototype,"data",void 0),Et=t([dt("period-summary-chart")],Et);let Tt=class extends nt{constructor(){super(...arguments),this.entryId="",this._data=null,this._periodData=null,this._loading=!1,this._error=""}updated(t){t.has("hass")&&this.hass&&this.entryId&&!this._data&&!this._loading&&this._fetchData()}async _fetchData(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{const[t,e]=await Promise.all([this.hass.callWS({type:"my_solar_cells/get_overview",entry_id:this.entryId}),this.hass.callWS({type:"my_solar_cells/get_period_summaries",entry_id:this.entryId})]);this._data=t,this._periodData=e}catch(t){this._error=t.message||"Failed to fetch data"}this._loading=!1}}render(){if(this._loading)return N`<div class="loading">${St(this.hass,"overview.loadingOverview")}</div>`;if(this._error)return N`<div class="no-data">${St(this.hass,"common.error")}: ${this._error}</div>`;if(!this._data)return N`<div class="no-data">${St(this.hass,"overview.noData")}</div>`;const t=this._data;return N`
      <div class="card">
        <h3>${St(this.hass,"overview.dbSummary")}</h3>
        <div class="stats-grid">
          <div class="stat-item">
            <div class="stat-label">${St(this.hass,"overview.lastTibberSync")}</div>
            <div class="stat-value">${t.last_tibber_sync?this._formatTimestamp(t.last_tibber_sync):St(this.hass,"common.never")}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">${St(this.hass,"overview.hourlyRecords")}</div>
            <div class="stat-value">${t.hourly_record_count.toLocaleString(xt(this.hass))}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">${St(this.hass,"overview.firstRecord")}</div>
            <div class="stat-value">${t.first_timestamp?this._formatDate(t.first_timestamp):St(this.hass,"common.na")}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">${St(this.hass,"overview.lastRecord")}</div>
            <div class="stat-value">${t.last_timestamp?this._formatDate(t.last_timestamp):St(this.hass,"common.na")}</div>
          </div>
        </div>
      </div>

      ${this._periodData?N`
            <div class="card">
              <h3>${St(this.hass,"overview.energySummary")}</h3>
              <period-summary-chart .hass=${this.hass} .data=${this._periodData}></period-summary-chart>
            </div>
          `:H}

      ${this._renderYearlyParams(t.yearly_params)}
    `}_renderYearlyParams(t){const e=Object.keys(t).sort();return 0===e.length?H:N`
      <div class="card">
        <h3>${St(this.hass,"overview.yearlyParams")}</h3>
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>${St(this.hass,"common.year")}</th>
                <th>${St(this.hass,"overview.taxReduction")}</th>
                <th>${St(this.hass,"overview.gridComp")}</th>
                <th>${St(this.hass,"overview.transferFee")}</th>
                <th>${St(this.hass,"overview.energyTax")}</th>
                <th>${St(this.hass,"overview.installedKw")}</th>
              </tr>
            </thead>
            <tbody>
              ${e.map(e=>{const a=t[e];return N`
                  <tr>
                    <td>${e}</td>
                    <td>${this._fmt(a.tax_reduction)}</td>
                    <td>${this._fmt(a.grid_compensation)}</td>
                    <td>${this._fmt(a.transfer_fee)}</td>
                    <td>${this._fmt(a.energy_tax)}</td>
                    <td>${null!=a.installed_kw?a.installed_kw:"-"}</td>
                  </tr>
                `})}
            </tbody>
          </table>
        </div>
      </div>
    `}_fmt(t){return null!=t?t.toFixed(3):"-"}_formatTimestamp(t){try{return new Date(t).toLocaleString(xt(this.hass))}catch{return t}}_formatDate(t){try{return t.substring(0,10)}catch{return t}}};Tt.styles=[gt,vt],t([pt({attribute:!1})],Tt.prototype,"hass",void 0),t([pt()],Tt.prototype,"entryId",void 0),t([ut()],Tt.prototype,"_data",void 0),t([ut()],Tt.prototype,"_periodData",void 0),t([ut()],Tt.prototype,"_loading",void 0),t([ut()],Tt.prototype,"_error",void 0),Tt=t([dt("overview-view")],Tt);const It=50;let Rt=class extends nt{constructor(){super(...arguments),this.entryId="",this._startDate="",this._endDate="",this._records=[],this._totalCount=0,this._offset=0,this._loading=!1,this._error=""}connectedCallback(){super.connectedCallback();const t=(new Date).toISOString().substring(0,10);this._startDate=t,this._endDate=t}render(){return N`
      <div class="card">
        <h3>${St(this.hass,"hourly.title")}</h3>
        <div class="table-controls">
          <div class="input-group">
            <label>${St(this.hass,"hourly.startDate")}</label>
            <input
              type="date"
              .value=${this._startDate}
              @change=${t=>{this._startDate=t.target.value}}
            />
          </div>
          <div class="input-group">
            <label>${St(this.hass,"hourly.endDate")}</label>
            <input
              type="date"
              .value=${this._endDate}
              @change=${t=>{this._endDate=t.target.value}}
            />
          </div>
          <button class="btn" @click=${this._fetch} ?disabled=${this._loading}>
            ${this._loading?St(this.hass,"hourly.loadingBtn"):St(this.hass,"hourly.loadBtn")}
          </button>
        </div>

        ${this._error?N`<div class="no-data">${St(this.hass,"common.error")}: ${this._error}</div>`:""}
        ${this._records.length>0?this._renderTable():""}
        ${this._loading||0!==this._records.length||this._error?"":N`<div class="no-data">
              ${St(this.hass,"hourly.selectDateRange")}
            </div>`}
      </div>
    `}_renderTable(){const t=Math.ceil(this._totalCount/It),e=Math.floor(this._offset/It)+1;return N`
      <div class="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>${St(this.hass,"hourly.timestamp")}</th>
              <th>${St(this.hass,"hourly.purchasedKwh")}</th>
              <th>${St(this.hass,"hourly.costSek")}</th>
              <th>${St(this.hass,"hourly.soldKwh")}</th>
              <th>${St(this.hass,"hourly.profitSek")}</th>
              <th>${St(this.hass,"hourly.ownUseKwh")}</th>
              <th>${St(this.hass,"hourly.savedSek")}</th>
              <th>${St(this.hass,"hourly.priceLevel")}</th>
              <th>${St(this.hass,"hourly.synced")}</th>
              <th>${St(this.hass,"hourly.enriched")}</th>
            </tr>
          </thead>
          <tbody>
            ${this._records.map(t=>N`
                <tr>
                  <td>${this._formatTs(t.timestamp)}</td>
                  <td>${t.purchased.toFixed(3)}</td>
                  <td>${t.purchased_cost.toFixed(2)}</td>
                  <td>${t.production_sold.toFixed(3)}</td>
                  <td>${t.production_sold_profit.toFixed(2)}</td>
                  <td>${t.production_own_use.toFixed(3)}</td>
                  <td>${t.production_own_use_profit.toFixed(2)}</td>
                  <td>${t.price_level||"-"}</td>
                  <td>${t.synced?St(this.hass,"common.yes"):St(this.hass,"common.no")}</td>
                  <td>${t.sensor_enriched?St(this.hass,"common.yes"):St(this.hass,"common.no")}</td>
                </tr>
              `)}
          </tbody>
        </table>
      </div>
      <div class="pagination">
        <button
          class="btn"
          ?disabled=${0===this._offset||this._loading}
          @click=${this._prevPage}
        >
          ${St(this.hass,"hourly.prev")}
        </button>
        <span>${St(this.hass,"hourly.pageInfo",e,t,this._totalCount)}</span>
        <button
          class="btn"
          ?disabled=${this._offset+It>=this._totalCount||this._loading}
          @click=${this._nextPage}
        >
          ${St(this.hass,"hourly.next")}
        </button>
      </div>
    `}async _fetch(){if(this.hass&&this.entryId&&this._startDate&&this._endDate){this._loading=!0,this._error="";try{const t=`${this._startDate}T00:00:00`,e=new Date(this._endDate);e.setDate(e.getDate()+1);const a=`${e.toISOString().substring(0,10)}T00:00:00`,s=await this.hass.callWS({type:"my_solar_cells/get_hourly_energy",entry_id:this.entryId,start_date:t,end_date:a,offset:this._offset,limit:It});this._records=s.records,this._totalCount=s.total_count}catch(t){this._error=t.message||"Failed to fetch data",this._records=[],this._totalCount=0}this._loading=!1}}_prevPage(){this._offset=Math.max(0,this._offset-It),this._fetch()}_nextPage(){this._offset+=It,this._fetch()}_formatTs(t){try{return t.replace("T"," ").substring(0,19)}catch{return t}}};Rt.styles=[gt,vt],t([pt({attribute:!1})],Rt.prototype,"hass",void 0),t([pt()],Rt.prototype,"entryId",void 0),t([ut()],Rt.prototype,"_startDate",void 0),t([ut()],Rt.prototype,"_endDate",void 0),t([ut()],Rt.prototype,"_records",void 0),t([ut()],Rt.prototype,"_totalCount",void 0),t([ut()],Rt.prototype,"_offset",void 0),t([ut()],Rt.prototype,"_loading",void 0),t([ut()],Rt.prototype,"_error",void 0),Rt=t([dt("hourly-energy-view")],Rt);let Ct=class extends nt{constructor(){super(...arguments),this.entryId="",this._sensors=[],this._loading=!1,this._error=""}updated(t){t.has("hass")&&this.hass&&this.entryId&&!this._sensors.length&&!this._loading&&this._fetchData()}async _fetchData(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{const t=await this.hass.callWS({type:"my_solar_cells/get_sensor_config",entry_id:this.entryId});this._sensors=t.sensors}catch(t){this._error=t.message||"Failed to fetch sensor config"}this._loading=!1}}render(){if(this._loading)return N`<div class="loading">${St(this.hass,"sensors.loadingSensors")}</div>`;if(this._error)return N`<div class="no-data">${St(this.hass,"common.error")}: ${this._error}</div>`;this._sensors.filter(t=>t.entity_id);const t=this._sensors.filter(t=>!t.entity_id);return N`
      <div class="info-box">${this._renderInfoBox()}</div>

      <div class="card">
        <h3>${St(this.hass,"sensors.title")}</h3>
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>${St(this.hass,"sensors.status")}</th>
                <th>${St(this.hass,"sensors.role")}</th>
                <th>${St(this.hass,"sensors.description")}</th>
                <th>${St(this.hass,"sensors.entityId")}</th>
                <th>${St(this.hass,"sensors.currentState")}</th>
                <th>${St(this.hass,"sensors.lastStoredReading")}</th>
              </tr>
            </thead>
            <tbody>
              ${this._sensors.map(t=>this._renderRow(t))}
            </tbody>
          </table>
        </div>
      </div>

      ${t.some(t=>"production"===t.role)?N`
            <div class="card">
              <h3>${St(this.hass,"sensors.requiredMissing")}</h3>
              <p style="color: var(--error-color, #f44336); font-size: 0.9em;"
                >${this._renderRequiredMissingText()}</p>
            </div>
          `:H}

      <div style="margin-top: 12px;">
        <button class="btn" @click=${this._fetchData} ?disabled=${this._loading}>
          ${St(this.hass,"common.refresh")}
        </button>
      </div>
    `}_isRequired(t){return"production"===t}_getFallbackLabel(t){return St(this.hass,"grid_export"===t||"grid_import"===t?"sensors.usingTibberApi":"sensors.notConfigured")}_renderInfoBox(){return N`${this._unsafeHtml(St(this.hass,"sensors.infoBox"))}`}_renderRequiredMissingText(){return N`${this._unsafeHtml(St(this.hass,"sensors.requiredMissingText"))}`}_unsafeHtml(t){const e=document.createElement("span");return e.innerHTML=t,N`${e}`}_renderRow(t){const e=!!t.entity_id,a=this._isRequired(t.role);return N`
      <tr>
        <td>
          <span class="status-dot ${e?"configured":a?"missing":"optional"}"></span>
        </td>
        <td>
          <strong>${t.role}</strong>
          ${a?N`<span class="required-badge">${St(this.hass,"sensors.required")}</span>`:N`<span class="optional-badge">${St(this.hass,"sensors.optional")}</span>`}
        </td>
        <td>${t.description}</td>
        <td>
          ${e?N`<span class="entity-id">${t.entity_id}</span>`:N`<span class="fallback-label">${this._getFallbackLabel(t.role)}</span>`}
        </td>
        <td>
          ${null!=t.current_state?t.current_state:N`<span class="not-configured">-</span>`}
        </td>
        <td>
          ${null!=t.last_stored_reading?t.last_stored_reading.toFixed(3):N`<span class="not-configured">-</span>`}
        </td>
      </tr>
    `}};Ct.styles=[gt,vt,o`
      .status-dot {
        display: inline-block;
        width: 10px;
        height: 10px;
        border-radius: 50%;
        margin-right: 8px;
      }
      .status-dot.configured {
        background: var(--success-color, #4caf50);
      }
      .status-dot.missing {
        background: var(--error-color, #f44336);
      }
      .status-dot.optional {
        background: var(--warning-color, #ff9800);
      }
      .fallback-label {
        color: var(--secondary-text-color);
        font-size: 0.85em;
        font-style: italic;
      }
      .required-badge {
        display: inline-block;
        font-size: 0.7em;
        padding: 1px 6px;
        border-radius: 4px;
        background: var(--error-color, #f44336);
        color: white;
        font-weight: 500;
        margin-left: 6px;
        vertical-align: middle;
      }
      .optional-badge {
        display: inline-block;
        font-size: 0.7em;
        padding: 1px 6px;
        border-radius: 4px;
        background: var(--secondary-text-color);
        color: white;
        font-weight: 500;
        margin-left: 6px;
        vertical-align: middle;
      }
      .entity-id {
        font-family: monospace;
        font-size: 0.85em;
        color: var(--primary-text-color);
      }
      .not-configured {
        color: var(--secondary-text-color);
        font-style: italic;
      }
      .info-box {
        background: var(--primary-background-color);
        border-radius: 8px;
        padding: 12px 16px;
        margin-bottom: 16px;
        font-size: 0.9em;
        color: var(--secondary-text-color);
        line-height: 1.5;
      }
    `],t([pt({attribute:!1})],Ct.prototype,"hass",void 0),t([pt()],Ct.prototype,"entryId",void 0),t([ut()],Ct.prototype,"_sensors",void 0),t([ut()],Ct.prototype,"_loading",void 0),t([ut()],Ct.prototype,"_error",void 0),Ct=t([dt("sensors-view")],Ct);const Ot={tax_reduction:.6,grid_compensation:.078,transfer_fee:.3,energy_tax:.49,installed_kw:10};let Lt=class extends nt{constructor(){super(...arguments),this.entryId="",this._params={},this._loading=!1,this._fetched=!1,this._error="",this._editingYear=null,this._editValues={...Ot},this._newYear="",this._saving=!1,this._minYear=0,this._maxYear=0}updated(t){(t.has("hass")||t.has("entryId"))&&this.hass&&this.entryId&&!this._loading&&!this._fetched&&this._fetchData()}async _fetchData(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{const t=await this.hass.callWS({type:"my_solar_cells/get_yearly_params",entry_id:this.entryId});this._params=t.yearly_params,t.first_timestamp&&(this._minYear=new Date(t.first_timestamp).getFullYear()),t.last_timestamp&&(this._maxYear=new Date(t.last_timestamp).getFullYear())}catch(t){this._error=t.message||"Failed to fetch yearly params"}this._loading=!1,this._fetched=!0}}render(){if(this._loading&&!this._fetched)return N`<div class="loading">${St(this.hass,"params.loadingParams")}</div>`;if(this._error)return N`<div class="no-data">${St(this.hass,"common.error")}: ${this._error}</div>`;if(!this._fetched)return N`<div class="no-data">${St(this.hass,"params.waitingForData")}</div>`;const t=Object.keys(this._params).sort();return N`
      <div class="card">
        <h3>${St(this.hass,"params.title")}</h3>

        <div class="add-year-row">
          <div class="input-group">
            <label>${St(this.hass,"params.addYear")}</label>
            <select
              .value=${this._newYear}
              @change=${t=>this._newYear=t.target.value}
            >
              <option value="">${St(this.hass,"params.selectYear")}</option>
              ${this._getAvailableYears().map(t=>N`<option value=${t}>${t}</option>`)}
            </select>
          </div>
          <button class="btn" @click=${this._addYear}>${St(this.hass,"common.add")}</button>
        </div>

        ${0===t.length?N`<div class="no-data">
              ${St(this.hass,"params.noParams")}
            </div>`:N`
              <div class="table-wrapper">
                <table>
                  <thead>
                    <tr>
                      <th>${St(this.hass,"common.year")}</th>
                      <th>${St(this.hass,"params.taxReduction")}</th>
                      <th>${St(this.hass,"params.gridComp")}</th>
                      <th>${St(this.hass,"params.transferFee")}</th>
                      <th>${St(this.hass,"params.energyTax")}</th>
                      <th>${St(this.hass,"params.installedKw")}</th>
                    </tr>
                  </thead>
                  <tbody>
                    ${t.map(t=>{const e=this._params[t],a=this._editingYear===t;return N`
                        <tr
                          class="clickable ${a?"selected":""}"
                          @click=${()=>this._startEdit(t)}
                        >
                          <td>${t}</td>
                          <td>${this._fmt(e.tax_reduction)}</td>
                          <td>${this._fmt(e.grid_compensation)}</td>
                          <td>${this._fmt(e.transfer_fee)}</td>
                          <td>${this._fmt(e.energy_tax)}</td>
                          <td>${null!=e.installed_kw?e.installed_kw:"-"}</td>
                        </tr>
                      `})}
                  </tbody>
                </table>
              </div>
            `}
      </div>

      ${null!=this._editingYear?this._renderEditForm():H}
    `}_renderEditForm(){const t=this._editValues;return N`
      <div class="card">
        <h3>${St(this.hass,"params.editYear",this._editingYear)}</h3>
        <div class="edit-form">
          <div class="input-group">
            <label>${St(this.hass,"params.taxReductionLabel")}</label>
            <input
              type="number"
              step="0.001"
              .value=${String(t.tax_reduction??"")}
              @input=${t=>this._editValues={...this._editValues,tax_reduction:parseFloat(t.target.value)}}
            />
          </div>
          <div class="input-group">
            <label>${St(this.hass,"params.gridCompLabel")}</label>
            <input
              type="number"
              step="0.001"
              .value=${String(t.grid_compensation??"")}
              @input=${t=>this._editValues={...this._editValues,grid_compensation:parseFloat(t.target.value)}}
            />
          </div>
          <div class="input-group">
            <label>${St(this.hass,"params.transferFeeLabel")}</label>
            <input
              type="number"
              step="0.001"
              .value=${String(t.transfer_fee??"")}
              @input=${t=>this._editValues={...this._editValues,transfer_fee:parseFloat(t.target.value)}}
            />
          </div>
          <div class="input-group">
            <label>${St(this.hass,"params.energyTaxLabel")}</label>
            <input
              type="number"
              step="0.001"
              .value=${String(t.energy_tax??"")}
              @input=${t=>this._editValues={...this._editValues,energy_tax:parseFloat(t.target.value)}}
            />
          </div>
          <div class="input-group">
            <label>${St(this.hass,"params.installedKwLabel")}</label>
            <input
              type="number"
              step="0.01"
              .value=${String(t.installed_kw??"")}
              @input=${t=>this._editValues={...this._editValues,installed_kw:parseFloat(t.target.value)}}
            />
          </div>
        </div>
        <div class="form-actions">
          <button class="btn" ?disabled=${this._saving} @click=${this._save}>
            ${this._saving?St(this.hass,"params.saving"):St(this.hass,"common.save")}
          </button>
          <button class="btn btn-secondary" @click=${this._cancelEdit}>
            ${St(this.hass,"common.cancel")}
          </button>
          <button class="btn btn-danger" @click=${this._delete}>${St(this.hass,"common.delete")}</button>
        </div>
      </div>
    `}_fmt(t){return null!=t?t.toFixed(3):"-"}_startEdit(t){this._editingYear=t;const e=this._params[t]||{};this._editValues={tax_reduction:e.tax_reduction??Ot.tax_reduction,grid_compensation:e.grid_compensation??Ot.grid_compensation,transfer_fee:e.transfer_fee??Ot.transfer_fee,energy_tax:e.energy_tax??Ot.energy_tax,installed_kw:e.installed_kw??Ot.installed_kw}}_cancelEdit(){this._editingYear=null}_getAvailableYears(){const t=[];for(let e=this._minYear;e<=this._maxYear;e++)this._params[String(e)]||t.push(e);return t}_addYear(){const t=parseInt(this._newYear,10);if(isNaN(t))return;const e=String(t);if(this._params[e])return this._startEdit(e),void(this._newYear="");const a=Object.keys(this._params).filter(t=>t<e).sort(),s=a.length>0?this._params[a[a.length-1]]:null;this._editValues=s?{...s}:{...Ot},this._editingYear=e,this._newYear=""}async _save(){if(null!=this._editingYear&&this.hass&&this.entryId){this._saving=!0;try{await this.hass.callWS({type:"my_solar_cells/set_yearly_params",entry_id:this.entryId,year:parseInt(this._editingYear,10),tax_reduction:this._editValues.tax_reduction??0,grid_compensation:this._editValues.grid_compensation??0,transfer_fee:this._editValues.transfer_fee??0,energy_tax:this._editValues.energy_tax??0,installed_kw:this._editValues.installed_kw??0}),this._editingYear=null,await this._fetchData()}catch(t){this._error=t.message||"Failed to save"}this._saving=!1}}async _delete(){if(null!=this._editingYear&&this.hass&&this.entryId&&confirm(St(this.hass,"params.deleteConfirm",this._editingYear))){this._saving=!0;try{await this.hass.callWS({type:"my_solar_cells/delete_yearly_params",entry_id:this.entryId,year:parseInt(this._editingYear,10)}),this._editingYear=null,await this._fetchData()}catch(t){this._error=t.message||"Failed to delete"}this._saving=!1}}};Lt.styles=[gt,vt,o`
      .edit-form {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
        gap: 12px;
        margin-top: 12px;
      }

      .edit-form .input-group input {
        width: 100%;
        box-sizing: border-box;
      }

      .form-actions {
        display: flex;
        gap: 8px;
        margin-top: 16px;
        align-items: center;
      }

      .btn-danger {
        background: var(--error-color, #db4437) !important;
      }

      .btn-secondary {
        background: var(--secondary-text-color) !important;
      }

      .add-year-row {
        display: flex;
        gap: 8px;
        align-items: flex-end;
        margin-bottom: 16px;
      }

      .add-year-row .input-group select {
        width: 100px;
        padding: 6px 8px;
        border: 1px solid var(--divider-color, #e0e0e0);
        border-radius: 4px;
        background: var(--card-background-color, #fff);
        color: var(--primary-text-color);
        font-size: 14px;
      }

      tr.clickable {
        cursor: pointer;
      }

      tr.clickable:hover td {
        background: var(--primary-background-color);
      }

      tr.selected td {
        background: color-mix(in srgb, var(--primary-color) 12%, transparent);
      }
    `],t([pt({attribute:!1})],Lt.prototype,"hass",void 0),t([pt()],Lt.prototype,"entryId",void 0),t([ut()],Lt.prototype,"_params",void 0),t([ut()],Lt.prototype,"_loading",void 0),t([ut()],Lt.prototype,"_fetched",void 0),t([ut()],Lt.prototype,"_error",void 0),t([ut()],Lt.prototype,"_editingYear",void 0),t([ut()],Lt.prototype,"_editValues",void 0),t([ut()],Lt.prototype,"_newYear",void 0),t([ut()],Lt.prototype,"_saving",void 0),t([ut()],Lt.prototype,"_minYear",void 0),t([ut()],Lt.prototype,"_maxYear",void 0),Lt=t([dt("yearly-params-view")],Lt);let Ut=class extends nt{constructor(){super(...arguments),this.entryId="",this._projection=[],this._investment=0,this._loading=!1,this._error="",this._initialLoaded=!1,this._defaultPriceDev=5,this._defaultPanelDeg=.25}updated(t){t.has("hass")&&this.hass&&this.entryId&&!this._initialLoaded&&!this._loading&&this._fetchInitial()}async _fetchInitial(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{const t=await this.hass.callWS({type:"my_solar_cells/get_roi_projection",entry_id:this.entryId});this._projection=t.projection,this._investment=t.investment,this._defaultPriceDev=t.price_development,this._defaultPanelDeg=t.panel_degradation,this._initialLoaded=!0}catch(t){this._error=t.message||"Failed to fetch ROI projection"}this._loading=!1}}async _onCalculate(){if(!this.hass||!this.entryId)return;const t=this.shadowRoot.getElementById("price-dev-input"),e=this.shadowRoot.getElementById("panel-deg-input"),a=parseFloat(t.value)||0,s=parseFloat(e.value)||0;this._loading=!0,this._error="";try{const t=await this.hass.callWS({type:"my_solar_cells/get_roi_projection",entry_id:this.entryId,price_development:a,panel_degradation:s});this._projection=t.projection,this._investment=t.investment}catch(t){this._error=t.message||"Failed to recalculate ROI projection"}this._loading=!1}_fmtInt(t){return Math.round(t).toLocaleString(xt(this.hass))}_fmtSek(t){return t.toLocaleString(xt(this.hass),{minimumFractionDigits:2,maximumFractionDigits:2})}_fmtPct(t){return t.toLocaleString(xt(this.hass),{minimumFractionDigits:1,maximumFractionDigits:1})}render(){return this._loading&&!this._initialLoaded?N`<div class="loading">${St(this.hass,"roi.loadingRoi")}</div>`:this._error?N`<div class="no-data">${St(this.hass,"common.error")}: ${this._error}</div>`:this._projection.length?N`
      <div class="card">
        <h3>${St(this.hass,"roi.title")}</h3>
        <div class="investment-info">
          ${this._renderInvestment()}
        </div>
        <details class="info-box">
          <summary>${St(this.hass,"roi.howCalculated")}</summary>
          <ul>
            <li>${St(this.hass,"roi.explainHistorical")}</li>
            <li>${St(this.hass,"roi.explainSoldPrice")}</li>
            <li>${St(this.hass,"roi.explainOwnUsePrice")}</li>
            <li>${St(this.hass,"roi.explainFuture")}
            <ul>
              <li>${St(this.hass,"roi.explainPriceDev")}</li>
              <li>${St(this.hass,"roi.explainPanelDeg")}</li>
            </ul>
            </li>
            <li>${St(this.hass,"roi.explainSavingsSold")}</li>
            <li>${St(this.hass,"roi.explainSavingsOwnUse")}</li>
            <li>${St(this.hass,"roi.explainRemaining")}</li>
            <li>${St(this.hass,"roi.explainRoiYear")}</li>
            <li>${St(this.hass,"roi.explainTaxReduction")}</li>
            <li><em>${St(this.hass,"roi.explainNote")}</em></li>
          </ul>
        </details>
        <div class="table-controls">
          <div class="input-group">
            <label>${St(this.hass,"roi.priceDevLabel")}</label>
            <input
              id="price-dev-input"
              type="number"
              step="0.5"
              value=${this._defaultPriceDev}
            />
          </div>
          <div class="input-group">
            <label>${St(this.hass,"roi.panelDegLabel")}</label>
            <input
              id="panel-deg-input"
              type="number"
              step="0.05"
              value=${this._defaultPanelDeg}
            />
          </div>
          <button class="btn" @click=${this._onCalculate} ?disabled=${this._loading}>
            ${this._loading?St(this.hass,"roi.calculating"):St(this.hass,"roi.calculate")}
          </button>
        </div>
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th class="number">${St(this.hass,"roi.colIndex")}</th>
                <th class="number">${St(this.hass,"roi.colYear")}</th>
                <th class="number">${St(this.hass,"roi.colAvgPriceSold")}</th>
                <th class="number">${St(this.hass,"roi.colAvgPriceOwnUse")}</th>
                <th class="number">${St(this.hass,"roi.colProdSold")}</th>
                <th class="number">${St(this.hass,"roi.colProdOwnUse")}</th>
                <th class="number">${St(this.hass,"roi.colSavingsSold")}</th>
                <th class="number">${St(this.hass,"roi.colSavingsOwnUse")}</th>
                <th class="number">${St(this.hass,"roi.colReturnPct")}</th>
                <th class="number">${St(this.hass,"roi.colRemaining")}</th>
              </tr>
            </thead>
            <tbody>
              ${this._projection.map(t=>this._renderRow(t))}
            </tbody>
          </table>
        </div>
      </div>
    `:N`<div class="no-data">${St(this.hass,"roi.noData")}</div>`}_renderInvestment(){const t=St(this.hass,"roi.investment",this._fmtSek(this._investment)),e=document.createElement("span");return e.innerHTML=t,N`${e}`}_renderRow(t){return N`
      <tr class=${t.is_roi_year?"roi-row":""}>
        <td class="number">${t.year_from_start}</td>
        <td class="number">${t.year}</td>
        <td class="number">${this._fmtSek(t.average_price_sold)}</td>
        <td class="number">${this._fmtSek(t.average_price_own_use)}</td>
        <td class="number">${this._fmtInt(t.production_sold)}</td>
        <td class="number">${this._fmtInt(t.production_own_use)}</td>
        <td class="number">${this._fmtSek(t.year_savings_sold)}</td>
        <td class="number">${this._fmtSek(t.year_savings_own_use)}</td>
        <td class="number">${this._fmtPct(t.return_percentage)}</td>
        <td class="number">${this._fmtSek(t.remaining_on_investment)}</td>
      </tr>
    `}};Ut.styles=[gt,vt,o`
      .roi-row {
        background: rgba(76, 175, 80, 0.15);
        font-weight: 600;
      }

      td.number {
        text-align: right;
        font-variant-numeric: tabular-nums;
      }

      th.number {
        text-align: right;
      }

      .investment-info {
        font-size: 0.95em;
        color: var(--secondary-text-color);
        margin-bottom: 12px;
      }

      .investment-info strong {
        color: var(--primary-text-color);
      }

      .input-group input[type="number"] {
        width: 80px;
      }

      .info-box {
        background: var(--primary-background-color);
        border-left: 3px solid var(--primary-color);
        border-radius: 4px;
        padding: 12px 16px;
        margin-bottom: 16px;
        font-size: 0.85em;
        line-height: 1.5;
        color: var(--secondary-text-color);
      }

      .info-box summary {
        cursor: pointer;
        color: var(--primary-text-color);
        font-weight: 500;
      }

      .info-box ul {
        margin: 8px 0 0 0;
        padding-left: 20px;
      }

      .info-box li {
        margin-bottom: 4px;
      }
    `],t([pt({attribute:!1})],Ut.prototype,"hass",void 0),t([pt()],Ut.prototype,"entryId",void 0),t([ut()],Ut.prototype,"_projection",void 0),t([ut()],Ut.prototype,"_investment",void 0),t([ut()],Ut.prototype,"_loading",void 0),t([ut()],Ut.prototype,"_error",void 0),t([ut()],Ut.prototype,"_initialLoaded",void 0),Ut=t([dt("roi-view")],Ut);const Kt=[{type:"today",labelKey:"fakta.periodToday"},{type:"day",labelKey:"fakta.periodDay"},{type:"week",labelKey:"fakta.periodWeek"},{type:"month",labelKey:"fakta.periodMonth"},{type:"year",labelKey:"fakta.periodYear"}];let Ft=class extends nt{constructor(){super(...arguments),this.entryId="",this._period="week",this._currentDate=new Date,this._data=null,this._simData=null,this._loading=!1,this._simLoading=!1,this._error="",this._simEnabled=!1,this._simAddBattery=!0,this._simBatteryKwh=10,this._simRemoveTax=!1,this._initialLoaded=!1}updated(t){t.has("hass")&&this.hass&&this.entryId&&!this._initialLoaded&&!this._loading&&this._fetchData()}_getDateRange(){const t=new Date(this._currentDate);let e,a;switch(this._period){case"today":{const t=new Date;e=new Date(t.getFullYear(),t.getMonth(),t.getDate()),a=new Date(e),a.setDate(a.getDate()+1);break}case"day":e=new Date(t.getFullYear(),t.getMonth(),t.getDate()),a=new Date(e),a.setDate(a.getDate()+1);break;case"week":{const s=t.getDay(),r=0===s?6:s-1;e=new Date(t.getFullYear(),t.getMonth(),t.getDate()-r),a=new Date(e),a.setDate(a.getDate()+7);break}case"month":e=new Date(t.getFullYear(),t.getMonth(),1),a=new Date(t.getFullYear(),t.getMonth()+1,1);break;case"year":e=new Date(t.getFullYear(),0,1),a=new Date(t.getFullYear()+1,0,1)}return{start:this._toLocalIso(e),end:this._toLocalIso(a)}}_toLocalIso(t){return`${t.getFullYear()}-${String(t.getMonth()+1).padStart(2,"0")}-${String(t.getDate()).padStart(2,"0")}T00:00:00`}_getPeriodLabel(){const t=new Date(this._currentDate);switch(this._period){case"today":return St(this.hass,"fakta.todayLabel");case"day":return`${String(t.getDate()).padStart(2,"0")}/${At(this.hass,t.getMonth())} ${t.getFullYear()}`;case"week":{const e=t.getDay(),a=0===e?6:e-1,s=new Date(t.getFullYear(),t.getMonth(),t.getDate()-a),r=new Date(s);r.setDate(r.getDate()+6);return`${`${String(s.getDate()).padStart(2,"0")}/${At(this.hass,s.getMonth())}`}-${`${String(r.getDate()).padStart(2,"0")}/${At(this.hass,r.getMonth())}`}`}case"month":return`${e=this.hass,a=t.getMonth(),"sv"===wt(e)?bt[a]:ft[a]} ${t.getFullYear()}`;case"year":return`${t.getFullYear()}`}var e,a}_navigate(t){const e=new Date(this._currentDate);switch(this._period){case"day":e.setDate(e.getDate()+t);break;case"week":e.setDate(e.getDate()+7*t);break;case"month":e.setMonth(e.getMonth()+t);break;case"year":e.setFullYear(e.getFullYear()+t)}this._currentDate=e,this._fetchData()}_setPeriod(t){this._period=t,this._currentDate=new Date,this._simData=null,this._fetchData()}async _fetchData(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{const t=this._getDateRange(),e=await this.hass.callWS({type:"my_solar_cells/get_fakta_breakdown",entry_id:this.entryId,start_date:t.start,end_date:t.end});this._data=e,this._initialLoaded=!0}catch(t){this._error=t.message||"Failed to fetch data"}this._loading=!1}}async _simulate(){if(this.hass&&this.entryId){this._simLoading=!0;try{const t=this._getDateRange(),e=await this.hass.callWS({type:"my_solar_cells/simulate_fakta",entry_id:this.entryId,start_date:t.start,end_date:t.end,add_battery:this._simAddBattery,battery_kwh:this._simBatteryKwh,remove_tax_reduction:this._simRemoveTax});this._simData=e,this._simEnabled=!0}catch(t){this._error=t.message||"Simulation failed"}this._simLoading=!1}}_fmtKwh(t){return t.toLocaleString(xt(this.hass),{minimumFractionDigits:2,maximumFractionDigits:2})+" kWh"}_fmtSek(t){return t.toLocaleString(xt(this.hass),{minimumFractionDigits:2,maximumFractionDigits:2})+" SEK"}_fmtSekPerKwh(t){return t.toLocaleString(xt(this.hass),{minimumFractionDigits:2,maximumFractionDigits:2})+" SEK"}render(){if(this._loading&&!this._initialLoaded)return N`<div class="loading">Loading...</div>`;if(this._error&&!this._data)return N`<div class="no-data">Error: ${this._error}</div>`;const t=this._simEnabled&&this._simData?this._simData:this._data;return N`
      ${this._renderPeriodNav()}
      ${t?this._renderColumns(t):N`<div class="no-data">No data for this period</div>`}
    `}_renderPeriodNav(){const t="today"!==this._period;return N`
      <div class="period-nav">
        <div class="period-tabs">
          ${Kt.map(t=>N`
              <button
                class="period-tab ${this._period===t.type?"active":""}"
                @click=${()=>this._setPeriod(t.type)}
              >
                ${St(this.hass,t.labelKey)}
              </button>
            `)}
        </div>
        ${t?N`
              <button class="nav-arrow" @click=${()=>this._navigate(-1)}>&larr;</button>
              <span class="period-label">${this._getPeriodLabel()}</span>
              <button class="nav-arrow" @click=${()=>this._navigate(1)}>&rarr;</button>
            `:H}
      </div>
    `}_renderColumns(t){return N`
      <div class="fakta-columns">
        ${this._renderProductionColumn(t)}
        ${this._renderCostColumn(t)}
        ${this._renderSimAndFactsColumn(t)}
      </div>
    `}_renderProductionColumn(t){return N`
      <div class="fakta-card">
        <h3>${St(this.hass,"fakta.productionTitle")}</h3>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.sold")}</span><span class="value">${this._fmtKwh(t.production_sold)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.ownUse")}</span><span class="value">${this._fmtKwh(t.production_own_use)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.batteryCharge")}</span><span class="value">${this._fmtKwh(t.battery_charge)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.batteryUse")}</span><span class="value">${this._fmtKwh(t.battery_used)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.purchased")}</span><span class="value">${this._fmtKwh(t.purchased)}</span></div>
        <hr class="fakta-separator" />
        <div class="fakta-row fakta-summary"><span class="label">${St(this.hass,"fakta.production")}</span><span class="value">${this._fmtKwh(t.sum_all_production)}</span></div>
        <div class="fakta-row fakta-summary"><span class="label">${St(this.hass,"fakta.consumption")}</span><span class="value">${this._fmtKwh(t.sum_all_consumption)}</span></div>
        <div class="fakta-row fakta-summary"><span class="label">${St(this.hass,"fakta.balance")}</span><span class="value">${this._fmtKwh(t.sum_all_production-t.sum_all_consumption)}</span></div>
      </div>
    `}_renderCostColumn(t){return N`
      <div class="fakta-card">
        <h3>${St(this.hass,"fakta.costTitle")}</h3>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.prodSold")}</span><span class="value">${this._fmtSek(t.production_sold_profit)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.prodGridComp")}</span><span class="value">${this._fmtSek(t.production_sold_grid_compensation_profit)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.prodTaxReduction")}</span><span class="value">${this._fmtSek(t.production_sold_tax_reduction_profit)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.ownUseSpot")}</span><span class="value">${this._fmtSek(t.production_own_use_saved)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.ownUseTransfer")}</span><span class="value">${this._fmtSek(t.production_own_use_transfer_fee_saved)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.ownUseEnergyTax")}</span><span class="value">${this._fmtSek(t.production_own_use_energy_tax_saved)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.battSpot")}</span><span class="value">${this._fmtSek(t.battery_used_saved)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.battTransfer")}</span><span class="value">${this._fmtSek(t.battery_use_transfer_fee_saved)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.battEnergyTax")}</span><span class="value">${this._fmtSek(t.battery_use_energy_tax_saved)}</span></div>
        <hr class="fakta-separator" />
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.purchasedCost")}</span><span class="value">${this._fmtSek(-t.purchased_cost)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.purchasedTransfer")}</span><span class="value">${this._fmtSek(-t.purchased_transfer_fee_cost)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.purchasedTax")}</span><span class="value">${this._fmtSek(-t.purchased_tax_cost)}</span></div>
        <hr class="fakta-separator" />
        <div class="fakta-row fakta-summary"><span class="label">${St(this.hass,"fakta.costProduction")}</span><span class="value">${this._fmtSek(t.sum_all_production_sold_and_saved)}</span></div>
        <div class="fakta-row fakta-summary"><span class="label">${St(this.hass,"fakta.costConsumption")}</span><span class="value">${this._fmtSek(-t.sum_purchased_cost)}</span></div>
        <div class="fakta-row fakta-summary"><span class="label">${St(this.hass,"fakta.costBalance")}</span><span class="value">${this._fmtSek(t.balance)}</span></div>
      </div>
    `}_renderSimAndFactsColumn(t){return N`
      <div class="fakta-card">
        <h3>${this._simEnabled&&this._simData?this._simAddBattery?St(this.hass,"fakta.simTitleWithBattery",this._simBatteryKwh):St(this.hass,"fakta.simTitleWithoutBattery"):St(this.hass,"fakta.simTitle")}</h3>
        <div class="sim-section">
          <div class="sim-toggle-group">
            <button
              class="sim-toggle ${this._simAddBattery?"active":""}"
              @click=${()=>{this._simAddBattery=!0}}
            >${St(this.hass,"fakta.simAddBattery")}</button>
            <button
              class="sim-toggle ${this._simAddBattery?"":"active"}"
              @click=${()=>{this._simAddBattery=!1}}
            >${St(this.hass,"fakta.simRemoveBattery")}</button>
          </div>
          ${this._simAddBattery?N`
                <div class="sim-slider-row">
                  <input
                    type="range"
                    min="1"
                    max="30"
                    step="1"
                    .value=${String(this._simBatteryKwh)}
                    @input=${t=>{this._simBatteryKwh=parseInt(t.target.value)}}
                  />
                  <span class="slider-value">${this._simBatteryKwh} kWh</span>
                </div>
              `:H}
          <button
            class="sim-btn"
            @click=${this._simulate}
            ?disabled=${this._simLoading}
          >
            ${this._simLoading?St(this.hass,"fakta.simCalculating"):St(this.hass,"fakta.simCalculate")}
          </button>
        </div>

        <h3>${St(this.hass,"fakta.factsTitle")}</h3>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.factsProductionIndex")}</span><span class="value">${this._fmtKwh(t.facts_production_index)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.factsAvgPriceSold")}</span><span class="value">${this._fmtSekPerKwh(t.facts_production_sold_avg_per_kwh_profit)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.factsAvgPricePurchased")}</span><span class="value">${this._fmtSekPerKwh(t.facts_purchased_cost_avg_per_kwh)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.factsAvgPriceOwnUse")}</span><span class="value">${this._fmtSekPerKwh(t.facts_production_own_use_avg_per_kwh_saved)}</span></div>
        <div class="fakta-row"><span class="label">${St(this.hass,"fakta.factsPeakReduction")}</span><span class="value">${this._fmtKwh(t.peak_energy_reduction)}</span></div>
      </div>
    `}};Ft.styles=[_t],t([pt({attribute:!1})],Ft.prototype,"hass",void 0),t([pt()],Ft.prototype,"entryId",void 0),t([ut()],Ft.prototype,"_period",void 0),t([ut()],Ft.prototype,"_currentDate",void 0),t([ut()],Ft.prototype,"_data",void 0),t([ut()],Ft.prototype,"_simData",void 0),t([ut()],Ft.prototype,"_loading",void 0),t([ut()],Ft.prototype,"_simLoading",void 0),t([ut()],Ft.prototype,"_error",void 0),t([ut()],Ft.prototype,"_simEnabled",void 0),t([ut()],Ft.prototype,"_simAddBattery",void 0),t([ut()],Ft.prototype,"_simBatteryKwh",void 0),t([ut()],Ft.prototype,"_simRemoveTax",void 0),t([ut()],Ft.prototype,"_initialLoaded",void 0),Ft=t([dt("fakta-view")],Ft);let Bt=class extends nt{constructor(){super(...arguments),this._activeTab="overview"}get _entryId(){return this.panel?.config?.entry_id||""}render(){return N`
      <div class="content">
        <div class="header">
          <h1>${St(this.hass,"panel.title")}</h1>
        </div>
        <div class="tabs">
          <button
            class="tab ${"overview"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="overview"}
          >
            ${St(this.hass,"tab.overview")}
          </button>
          <button
            class="tab ${"roi"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="roi"}
          >
            ${St(this.hass,"tab.roi")}
          </button>
          <button
            class="tab ${"fakta"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="fakta"}
          >
            ${St(this.hass,"tab.fakta")}
          </button>
          <button
            class="tab ${"sensors"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="sensors"}
          >
            ${St(this.hass,"tab.sensors")}
          </button>
          <button
            class="tab ${"params"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="params"}
          >
            ${St(this.hass,"tab.yearlyParams")}
          </button>
          <button
            class="tab ${"hourly"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="hourly"}
          >
            ${St(this.hass,"tab.hourlyEnergy")}
          </button>
        </div>
        <div class="tab-content" ?active=${"overview"===this._activeTab}>
          <overview-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></overview-view>
        </div>
        <div class="tab-content" ?active=${"roi"===this._activeTab}>
          <roi-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></roi-view>
        </div>
        <div class="tab-content" ?active=${"fakta"===this._activeTab}>
          <fakta-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></fakta-view>
        </div>
        <div class="tab-content" ?active=${"sensors"===this._activeTab}>
          <sensors-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></sensors-view>
        </div>
        <div class="tab-content" ?active=${"params"===this._activeTab}>
          <yearly-params-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></yearly-params-view>
        </div>
        <div class="tab-content" ?active=${"hourly"===this._activeTab}>
          <hourly-energy-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></hourly-energy-view>
        </div>
      </div>
    `}};Bt.styles=[mt,o`
      .content {
        max-width: 1200px;
        margin: 0 auto;
      }

      .tab-content {
        display: none;
      }

      .tab-content[active] {
        display: block;
      }
    `],t([pt({attribute:!1})],Bt.prototype,"hass",void 0),t([pt({attribute:!1})],Bt.prototype,"narrow",void 0),t([pt({attribute:!1})],Bt.prototype,"route",void 0),t([pt({attribute:!1})],Bt.prototype,"panel",void 0),t([ut()],Bt.prototype,"_activeTab",void 0),Bt=t([dt("my-solar-cells-panel")],Bt);export{Bt as MySolarCellsPanel};
